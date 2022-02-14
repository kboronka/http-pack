using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using HttpPack.Json;

namespace HttpPack.Server;

public enum HttpMethod
{
    GET,
    HEAD,
    POST,
    PUT,
    DELETE
}

public class HttpRequest : HttpBase
{
    private int bytesRecived;

    // header
    private int contentLength;
    private bool headerRecived;

    private static byte[] CombineByteArrays(params byte[][] arrays)
    {
        var sum = 0;
        var offset = 0;

        foreach (var array in arrays)
        {
            sum += array.Length;
        }

        var result = new byte[sum];

        foreach (var array in arrays)
        {
            Buffer.BlockCopy(array, 0, result, offset, array.Length);
            offset += array.Length;
        }

        return result;
    }

    public override string ToString()
    {
        return Method + ": " + FullUrl;
    }

    #region constructor

    public HttpRequest(HttpConnection connection)
    {
        Server = connection.Parent;
        UserData = connection.Parent.UserData;
        RequestError = false;
        RemoteEndpoint = ((IPEndPoint) connection.Socket.Client.RemoteEndPoint).Address.ToString();
        ReadRequest(connection.Stream, connection.Socket);
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
    public object UserData { get; }
    public string RemoteEndpoint { get; }
    public string Authorization { get; private set; }
    public HttpMethod Method { get; private set; }
    public string ProtocolVersion { get; private set; }
    public string ContentType { get; private set; }
    public bool RequestError { get; }

    public string FullUrl { get; private set; }

    public string Query { get; private set; }

    public byte[] Data { get; private set; }

    public HttpServer Server { get; }

    public JsonKeyValuePairs Body
    {
        get
        {
            if (ContentType.Contains(@"application/json"))
            {
                var body = Encoding.UTF8.GetString(Data);
                return new JsonKeyValuePairs(body);
            }

            return new JsonKeyValuePairs();
        }
    }

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
                var incomingPacket = ReadIncomingPacket(stream, socket);
                buffer = CombineByteArrays(buffer, incomingPacket);

                if (buffer.Length > 0 && incomingPacket.Length == 0)
                {
                    // buffer is complete
                    ProcessIncomingBuffer(ref buffer);
                }
                else if (incomingPacket.Length != 0)
                {
                    // wait until entire request is received
                    Thread.Sleep(1);
                }

                Thread.Sleep(1);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        Response = new HttpResponse(this);
    }

    public byte[] ReadIncomingPacket(NetworkStream stream, TcpClient socket)
    {
        if (socket == null || stream == null)
        {
            return new byte[0] { };
        }

        lock (socket)
        {
            if (socket.Available > 0 && stream.DataAvailable)
            {
                var packetBytes = new byte[socket.Available];
                stream.Read(packetBytes, 0, packetBytes.Length);
                return packetBytes;
            }
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

        incomingRequestRecived |= contentLength == 0;
        ParseData(ref bufferIn);
        incomingRequestRecived |= bytesRecived >= contentLength;
    }

    private void ParseHeader(ref byte[] bufferIn)
    {
        if (headerRecived)
        {
            return;
        }

        // Request Line
        var requestLine = ReadLine(ref bufferIn);
        if (string.IsNullOrEmpty(requestLine))
        {
            throw new InvalidDataException("request line missing");
        }

        var initialRequest = requestLine.Split(' ');
        if (initialRequest.Length != 3)
        {
            throw new InvalidDataException("the initial request line should contain three fields");
        }

        FullUrl = CleanUrlString(initialRequest[1]);
        var url = FullUrl.Split('#');
        Path = StringHelper.TrimStart(url[0], 1);

        url = Path.Split('?');
        Path = url[0];
        Query = url.Length > 1 ? url[1] : "";

        ProtocolVersion = initialRequest[2];

        switch (initialRequest[0].ToUpper())
        {
            case "GET":
                Method = HttpMethod.GET;
                break;
            case "HEAD":
                Method = HttpMethod.HEAD;
                break;
            case "POST":
                Method = HttpMethod.POST;
                break;
            case "PUT":
                Method = HttpMethod.PUT;
                break;
            case "DELETE":
                Method = HttpMethod.DELETE;
                break;
            default:
                throw new InvalidDataException("unknown request type \"" + initialRequest[0] + "\"");
        }

        var line = "";
        var sessionID = "";

        while (!headerRecived)
        {
            line = ReadLine(ref bufferIn);

            headerRecived = string.IsNullOrEmpty(line);
            var requestHeader = line.Split(':');

            switch (requestHeader[0].TrimWhiteSpace().ToLower())
            {
                case "connection":
                    IsWebSocket = requestHeader[1].TrimWhiteSpace() == "Upgrade";
                    break;
                case "sec-websocket-key":
                    WebSocketKey = requestHeader[1].TrimWhiteSpace();
                    break;
                case "sec-websocket-protocol":
                    WebSocketProtocol = requestHeader[1].TrimWhiteSpace();
                    break;
                case "content-length":
                    contentLength = int.Parse(requestHeader[1]);
                    bytesRecived = 0;
                    break;
                case "user-agent":
                    break;
                case "content-type":
                    ContentType = requestHeader[1];
                    break;
                case "if-none-match":
                    ETag = requestHeader[1].TrimWhiteSpace();
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
                                Session = Server.Sessions[id];
                            }
                        }
                    }

                    break;
                case "authorization":
                    Authorization = requestHeader[1].TrimWhiteSpace();
                    break;
            }

            if (headerRecived)
            {
                break;
            }
        }

        if (Session == null)
        {
            Session = new HttpSession(Server);
            Session.SessionExpired += RemoveSession;
            lock (Server.Sessions)
            {
                Server.Sessions.Add(Session.ID, Session);
            }
        }

        Session.LastRequest = DateTime.Now;
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
        if (Method != HttpMethod.POST)
        {
            return;
        }

        // initialize data array
        if (bytesRecived == 0)
        {
            Data = new byte[contentLength];
        }

        Buffer.BlockCopy(bufferIn, bytesRecived, Data, bytesRecived, bufferIn.Length);
        bytesRecived += bufferIn.Length;
    }

    private static string ReadLine(ref byte[] bufferIn)
    {
        for (var i = 0; i < bufferIn.Length; i++)
        {
            if (bufferIn[i] == '\n')
            {
                var newBufferIn = new byte[bufferIn.Length - i - 1];
                Buffer.BlockCopy(bufferIn, i + 1, newBufferIn, 0, newBufferIn.Length);

                if (i > 0 && bufferIn[i - 1] == '\r')
                {
                    i--;
                }

                var line = Encoding.ASCII.GetString(bufferIn, 0, i);

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
            var index = url.LastIndexOf('%');
            var characterCode = url.Substring(index + 1, 2);
            var character = (char) Convert.ToInt32(characterCode, 16);

            url = url.Substring(0, index) + character + url.Substring(index + 3);
        }

        return url;
    }

    #endregion
}