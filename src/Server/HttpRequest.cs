/* Copyright (C) 2018 Kevin Boronka
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using HttpPack.Json;

namespace HttpPack.Server
{
	public enum HttpMethod
	{
		GET,
		POST,
		HEAD
	}
	
	public class HttpRequest : HttpBase
	{
		private readonly HttpServer parent;
		private HttpMethod method;
		private string fullUrl;
		private string query;
		private String protocolVersion;
		private bool headerRecived;
		
		// header
		private int contentLength;
		private int bytesRecived;
		private byte[] data;

		#region constructor
		
		public HttpRequest(HttpConnection connection)
		{
			this.parent = connection.Parent;
			this.UserData = connection.Parent.UserData;
			this.RequestError = false;
            this.RemoteEndpoint = ((IPEndPoint)connection.Socket.Client.RemoteEndPoint).Address.ToString();
            this.ReadRequest(connection.Stream, connection.Socket);
        }

        ~HttpRequest()
		{
		}
		
		#endregion
		
		#region properties

		public string Header { get; private set; }
		public string Path { get; set; }
		public string ETag { get; private set; }
		public HttpSession Session { get; private set; }
		public HttpResponse Response { get; private set; }
		public HttpWebSocket WebSocket { get; set; }
		public bool IsWebSocket { get; private set; }
		public string WebSocketKey { get; private set; }
		public string WebSocketProtocol { get; private set; }
		public object UserData { get; private set; }
        public string RemoteEndpoint { get; private set; }

        public HttpMethod Method
		{
			get { return method; }
		}

		public string FullUrl
		{
			get { return fullUrl; }
		}

		public string Query
		{
			get { return query; }
		}
		
		public byte[] Data
		{
			get { return data; }
		}
		
		private string json;
		public string Json
		{
			get
			{
				if (!String.IsNullOrEmpty(json))
				{
					return json;
				}
				
				json = JsonHelper.BytesToJson(data);
				return json;
			}
		}
		
		public string ProtocolVersion
		{
			get { return protocolVersion; }
		}
		
		public HttpServer Server
		{
			get { return parent; }
		}
		
		public bool RequestError { get; private set; }
		
		#endregion
		
		#region read request
		
		private bool incomingRequestRecived;

		private void ReadRequest(NetworkStream stream, TcpClient socket)
		{
			// Read and parse request
			var buffer = new byte[0] { };
			
			// TODO: add request timeout
			//var timeout = new Timing.Interval(10000);

			while (!incomingRequestRecived)
			{
				try
				{
					byte[] incomingPacket = this.ReadIncomingPacket(stream, socket);
					buffer = CombineByteArrays(buffer, incomingPacket);
					
					if (buffer.Length > 0 && incomingPacket.Length == 0)
					{
						// buffer is complete
						this.ProcessIncomingBuffer(ref buffer);
					}
					else if (incomingPacket.Length != 0)
					{
						// wait until entire request is recived
						Thread.Sleep(1);
					}
					
					Thread.Sleep(1);
				}
				catch (Exception ex)
				{
					throw(ex);
				}
			}
			
			this.Response = new HttpResponse(this);
		}
		
		public byte[] ReadIncomingPacket(NetworkStream stream, TcpClient socket)
		{
			try
			{
				if (socket == null || stream == null)
					return new byte[0] { };
				
				lock (socket)
				{
					if (socket.Available > 0 && stream.DataAvailable)
					{
						var packetBytes = new byte[socket.Available];
						stream.Read(packetBytes, 0, packetBytes.Length);
						return packetBytes;
					}
				}
			}
			catch (ObjectDisposedException)
			{
				throw;
				// The NetworkStream is closed.
			}
			catch (IOException)
			{
				throw;
				// The underlying Socket is closed.
			}
			catch (SocketException)
			{
				throw;
			}
			catch (Exception)
			{
				throw;
			}
			
			return new byte[0] { };
		}
		
		private void ProcessIncomingBuffer(ref byte[] bufferIn)
		{
			Header += StringHelper.GetString(bufferIn);
			
			ParseHeader(ref bufferIn);
			if (!headerRecived)
            {
                return;
            }
			
			incomingRequestRecived |= (this.contentLength == 0);
			ParseData(ref bufferIn);
			incomingRequestRecived |= this.bytesRecived >= this.contentLength;
		}
		
		private void ParseHeader(ref byte[] bufferIn)
		{
			if (headerRecived)
            {
                return;
            }

            // Request Line
            string requestLine = ReadLine(ref bufferIn);
			if (string.IsNullOrEmpty(requestLine))
			{
				throw new InvalidDataException("request line missing");
			}
			
			string[] initialRequest = requestLine.Split(' ');
			if (initialRequest.Length != 3)
			{
				throw new InvalidDataException("the initial request line should contain three fields");
			}

			this.fullUrl = CleanUrlString(initialRequest[1]);
			string[] url = this.fullUrl.Split('#');
			this.Path = StringHelper.TrimStart(url[0], 1);
			
			url = this.Path.Split('?');
			this.Path = url[0];
			this.query = url.Length > 1 ? url[1] : "";
			
			this.protocolVersion = initialRequest[2];
			
			switch (initialRequest[0].ToUpper())
			{
				case "GET":
					this.method = HttpMethod.GET;
					break;
				case "POST":
					this.method = HttpMethod.POST;
					break;
					//TODO: handle the HEAD request type
				default:
					throw new InvalidDataException("unknown request type \"" + initialRequest[0] + "\"");
			}
			
			string line = "";
			string sessionID = "";
			
			while (!this.headerRecived)
			{
				line = ReadLine(ref bufferIn);
				
				this.headerRecived = string.IsNullOrEmpty(line);
				
				string[] requestHeader = line.Split(':');
				#if DEBUG
				System.Diagnostics.Debug.WriteLine(line);
				#endif
				
				switch (requestHeader[0].TrimWhiteSpace().ToLower())
				{
					case "connection":
						this.IsWebSocket = requestHeader[1].TrimWhiteSpace() == "Upgrade";
						break;
					case "sec-websocket-key":
						this.WebSocketKey = requestHeader[1].TrimWhiteSpace();
						break;
					case "sec-websocket-protocol":
						this.WebSocketProtocol = requestHeader[1].TrimWhiteSpace();
						break;
					case "content-length":
						this.contentLength = int.Parse(requestHeader[1]);
						this.bytesRecived = 0;
						break;
					case "user-agent":
						break;
					case "content-type":
						break;
					case "if-none-match":
						this.ETag = requestHeader[1].TrimWhiteSpace();
						break;
					case "cookie":
						sessionID = requestHeader[1].TrimWhiteSpace();
						var matches = Regex.Matches(requestHeader[1], @"sarSession=([^;]+)");
						
						foreach (Match match in matches)
						{
							var id = match.Groups[1].Value;
							lock (Server.Sessions)
							{
								if (Server.Sessions.ContainsKey(id))
								{
									this.Session = Server.Sessions[id];
								}
							}
						}
						
						break;
				}
				
				if (this.headerRecived)
				{
					break;
				}
			}
			
			if (this.Session == null)
			{
				this.Session = new HttpSession(this.Server);
				this.Session.SessionExpired += new HttpSession.SessionExpiredHandler(this.RemoveSession);
				lock (Server.Sessions)
				{
					Server.Sessions.Add(this.Session.ID, this.Session);
				}
			}
			
			this.Session.LastRequest = DateTime.Now;
		}

		private void RemoveSession(HttpSession session)
		{
			try
			{
				lock (Server.Sessions)
				{
					Server.Sessions.Remove(session.ID);
				}
			}
			catch
			{
				
			}
		}
		
		private void ParseData(ref byte[] bufferIn)
		{
			if (this.method != HttpMethod.POST)
				return;
			
			// initilize data array
			if (this.bytesRecived == 0)
			{
				this.data = new Byte[this.contentLength];
			}
			
			System.Buffer.BlockCopy(bufferIn, this.bytesRecived, this.data, this.bytesRecived, bufferIn.Length);
			this.bytesRecived += bufferIn.Length;
		}
		
		private static string ReadLine(ref byte[] bufferIn)
		{
			for (int i = 0; i < bufferIn.Length; i++)
			{
				if (bufferIn[i] == '\n')
				{
					byte[] newBufferIn = new Byte[bufferIn.Length - i - 1];
					System.Buffer.BlockCopy(bufferIn, i + 1, newBufferIn, 0, newBufferIn.Length);
					
					if (i > 0 && bufferIn[i - 1] == '\r')
						i--;
					
					string line = Encoding.ASCII.GetString(bufferIn, 0, i);

					bufferIn = newBufferIn;
					return line;
				}
			}
			
			return null;
		}
		
		private static string CleanUrlString(string url)
		{
			while (url.Contains("%"))
			{
				int index = url.LastIndexOf('%');
				string characterCode = url.Substring(index + 1, 2);
				char character = (char)Convert.ToInt32(characterCode, 16);
				
				url = url.Substring(0, index) + character.ToString() + url.Substring(index + 3);
			}
			
			return url;
		}

        #endregion

        private static byte[] CombineByteArrays(params byte[][] arrays)
        {
            int sum = 0;
            int offset = 0;

            foreach (byte[] array in arrays)
            {
                sum += array.Length;
            }

            var result = new byte[sum];

            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }

            return result;
        }

        public override string ToString()
		{
			return this.method.ToString() + ": " + this.fullUrl;
		}
	}
}
