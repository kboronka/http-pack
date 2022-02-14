using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

namespace HttpPack
{
    public abstract class HttpWebSocket
    {
        private bool open;
        public HttpRequest request;

        private TcpClient socket;
        private NetworkStream stream;

        public HttpWebSocket(HttpRequest request)
        {
            open = true;
            this.request = request;
            ID = nextID++;
            OnNewClient(this);
        }

        public int ID { get; }

        public bool Open
        {
            get => open;
            set
            {
                if (!value && open)
                {
                    OnDisconnectedClient(this);
                    try
                    {
                        socket.Close();
                    }
                    catch
                    {
                    }
                }

                open = value;
            }
        }

        ~HttpWebSocket()
        {
        }

        public void SetSocket(TcpClient socket, NetworkStream stream)
        {
            this.socket = socket;
            this.stream = stream;
        }

        public abstract void NewData(string json);

        public void ReadNewData()
        {
            // Read and parse request
            var buffer = new byte[0] { };
            // TODO: add request timeout
            while (!request.RequestError)
            {
                try
                {
                    var incomingPacket = request.ReadIncomingPacket(stream, socket);
                    buffer = CombineByteArrays(buffer, incomingPacket);

                    if (buffer.Length > 0 && incomingPacket.Length == 0)
                    {
                        OnFrameRecived(HttpWebSocketFrame.DecodeFrame(buffer));
                        break;
                    }

                    if (incomingPacket.Length != 0)
                    {
                        // wait until entire request is recived
                        Thread.Sleep(1);
                    }

                    Thread.Sleep(1);
                }
                catch
                {
                    Open = false;
                    return;
                }
            }

            NewData(JsonHelper.BytesToJson(HttpWebSocketFrame.DecodeFrame(buffer).Payload));
        }

        private void Send(byte[] data)
        {
            // send response
            lock (socket)
            {
                try
                {
                    const int MAX_LENGTH = 8192;
                    for (var b = 0; b <= data.Length; b += MAX_LENGTH)
                    {
                        var length = Math.Min(data.Length - b, MAX_LENGTH);
                        stream.Write(data, b, length);
                    }

                    stream.Flush();
                }
                catch
                {
                    Open = false;
                }
            }
        }

        public void SendString(string message)
        {
            Send(HttpWebSocketFrame.EncodeFrame(message).EncodedFrame);
        }

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

        #region static

        [AttributeUsage(AttributeTargets.Class)]
        public class SarWebSocketController : Attribute
        {
        }

        private static Dictionary<string, Type> controllers;
        private static int nextID;

        public static void LoadControllers(List<Assembly> assemblies)
        {
            controllers = new Dictionary<string, Type>();

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.Name.EndsWith("WebSocket"))
                    {
                        foreach (var attribute in type.GetCustomAttributes(false))
                        {
                            if (attribute is SarWebSocketController)
                            {
                                // add the sar controller
                                var controllerName = type.Name.Substring(0, type.Name.Length - "WebSocket".Length);
                                controllers.Add(controllerName, type);
                            }
                        }
                    }
                }
            }
        }

        public static bool WebSocketControllerExists(HttpRequest request)
        {
            var urlSplit = request.Path.Split('/');
            if (urlSplit.Length != 1)
            {
                return false;
            }

            var controllerName = urlSplit[0];
            return controllers.ContainsKey(controllerName);
        }

        public static Type GetWebSocketController(HttpRequest request)
        {
            var urlSplit = request.Path.Split('/');
            if (urlSplit.Length != 1)
            {
                return null;
            }

            var controllerName = urlSplit[0];
            return controllers.ContainsKey(controllerName) ? controllers[controllerName] : null;
        }

        #endregion

        #region events

        #region new connection

        public delegate void ConnectedClientHandler(HttpWebSocket client);

        private static ConnectedClientHandler clientConnected;

        public static event ConnectedClientHandler ClientConnected
        {
            add => clientConnected += value;
            remove => clientConnected -= value;
        }

        private static void OnNewClient(HttpWebSocket client)
        {
            try
            {
                ConnectedClientHandler handler;
                if (null != (handler = clientConnected))
                {
                    handler(client);
                }
            }
            catch
            {
                client.Open = false;
            }
        }

        #endregion

        #region disconnected

        public delegate void ClientDisconnectedHandler(HttpWebSocket client);

        private static ClientDisconnectedHandler clientDisconnected;

        public static event ClientDisconnectedHandler ClientDisconnected
        {
            add => clientDisconnected += value;
            remove => clientDisconnected -= value;
        }

        private static void OnDisconnectedClient(HttpWebSocket client)
        {
            try
            {
                ClientDisconnectedHandler handler;
                if (null != (handler = clientDisconnected))
                {
                    handler(client);
                }
            }
            catch
            {
            }
        }

        #endregion

        #region frame recived

        public delegate void FrameRecivedHandler(HttpWebSocketFrame frame);

        private FrameRecivedHandler frameRecived;

        public event FrameRecivedHandler FrameRecived
        {
            add => frameRecived += value;
            remove => frameRecived -= value;
        }

        private void OnFrameRecived(HttpWebSocketFrame frame)
        {
            try
            {
                FrameRecivedHandler handler;
                if (null != (handler = frameRecived))
                {
                    handler(frame);
                }
            }
            catch
            {
                Open = false;
                Thread.Sleep(100);
            }
        }

        #endregion

        #endregion
    }
}