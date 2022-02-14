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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using HttpPack.Fsm;

namespace HttpPack
{
    public class HttpConnection : IDisposable
    {
#if DEBUG
        public const int MAX_TIME = 3;
#else
		public const int MAX_TIME = 300;
#endif

        private readonly Thread serviceRequestThread;
        private readonly Thread timeoutMonitorThread;
        private readonly Interval timeout;

        public bool Open { get; private set; }
        public bool Stopped { get; private set; }

        public HttpServer Parent { get; }
        public NetworkStream Stream { get; }
        public TcpClient Socket { get; set; }

        public HttpConnection(HttpServer parent, TcpClient socket)
        {
            Open = true;
            Socket = socket;
            Stream = socket.GetStream();

            Parent = parent;

            timeout = new Interval((MAX_TIME + 20) * 1000);
            var clientIp = ((IPEndPoint) socket.Client.RemoteEndPoint).Address.ToString();

            serviceRequestThread = new Thread(MonitorTimeout)
            {
                Name = "HttpConnection Service Request " + clientIp,
                IsBackground = true
            };
            serviceRequestThread.Start();

            timeoutMonitorThread = new Thread(ServiceRequests)
            {
                Name = "HttpConnection Timeout Monitor " + clientIp,
                Priority = ThreadPriority.Lowest,
                IsBackground = true
            };
            timeoutMonitorThread.Start();
        }

        ~HttpConnection()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private bool disposed;

        protected void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                // abort thread
                try
                {
                    serviceRequestThread.Abort();
                }
                catch
                {
                }

                // close connections
                try
                {
                    Stream.Close();
                    Socket.Close();
                }
                catch
                {
                }
            }

            disposed = true;
        }

        #region timeout monitor

        private void MonitorTimeout()
        {
            while (Open)
            {
                Thread.Sleep(1000);

                if (timeout.Ready)
                {
                    Open = false;
                }
            }
        }

        #endregion

        #region service request

        private bool RequestReady()
        {
            lock (Socket)
            {
                lock (Stream)
                {
                    if (Socket.Available > 0)
                    {
                        if (Stream.DataAvailable)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void ServiceRequests()
        {
            // Read and parse request
            while (Open)
            {
                try
                {
                    if (Socket.Connected && RequestReady())
                    {
                        timeout.Reset();

                        // return initial header
                        lock (Socket)
                        {
                            const string INIT_HEADER = "HTTP/1.1";
                            var bytes = Encoding.ASCII.GetBytes(INIT_HEADER);
                            Stream.Write(bytes, 0, bytes.Length);
                        }

                        // process request and get response
                        var request = new HttpRequest(this);

                        if (request.RequestError)
                        {
                            Open = false;
                            break;
                        }

                        var response = request.Response.bytes;

                        // send response
                        lock (Socket)
                        {
                            try
                            {
                                const int MAX_LENGTH = 8192;
                                for (var b = 0; b <= response.Length; b += MAX_LENGTH)
                                {
                                    var length = Math.Min(response.Length - b, MAX_LENGTH);
                                    Stream.Write(response, b, length);
                                }

                                Stream.Flush();
                            }
                            catch
                            {
                                // TODO: close connection?
                            }
                        }

                        if (request.IsWebSocket)
                        {
                            request.WebSocket.SetSocket(Socket, Stream);
                            while (request.WebSocket.Open && Socket.Connected)
                            {
                                // reset timeout
                                timeout.Reset();

                                if (RequestReady())
                                {
                                    request.WebSocket.ReadNewData();
                                }

                                Thread.Sleep(1);
                            }

                            request.WebSocket.Open = false;
                        }
                    }

                    Thread.Sleep(1);
                    Open &= Socket.Connected;
                }
                catch (Exception)
                {
                    //Logger.Log(ex);
                    Open = false;
                }
            }

            // close connections
            try
            {
                Stream.Close();
                Socket.Close();
            }
            catch
            {
            }

            Stopped = true;
        }

        #endregion
    }
}