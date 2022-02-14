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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

namespace HttpPack
{
    public class HttpServer : HttpBase
    {
        private AutoResetEvent connectionWaitHandle;
        private TcpListener listener;
        protected int port;
        protected string root;

        public static Assembly GetAssembly()
        {
            try
            {
                return Assembly.GetEntryAssembly();
            }
            catch
            {
            }

            try
            {
                return Assembly.GetCallingAssembly();
            }
            catch
            {
            }

            throw new ApplicationException("Assembly Not Found");
        }

        private static List<Assembly> GetReferencedAssemblies(Assembly assembly)
        {
            var assemblies = new List<Assembly>
            {
                assembly
            };

            foreach (var assemblyName in assembly.GetReferencedAssemblies())
            {
                var name = assemblyName.Name;

                if (!name.StartsWith("System")
                    && !name.StartsWith("mscorlib")
                    && !name.StartsWith("Microsoft.")
                    && !name.StartsWith("CefSharp."))
                {
                    try
                    {
                        assemblies.Add(Assembly.Load(assemblyName));
                    }
                    catch
                    {
                    }
                }
            }

            return assemblies;
        }

        #region constructor

        public HttpServer(int port, string wwwroot, object userData)
        {
            UserData = userData;
            this.port = port;
            if (wwwroot != null)
            {
                root = Path.GetFullPath(wwwroot);
            }

            var assembly = GetAssembly();
            Start(GetReferencedAssemblies(assembly));
        }

        private void Start(List<Assembly> assemblies)
        {
            if (root != null && !Directory.Exists(root))
            {
                throw new DirectoryNotFoundException("root folder not found");
            }

            Connections = new List<HttpConnection>();
            Sessions = new Dictionary<string, HttpSession>();

            Cache = new HttpCache(this);
            HttpController.LoadControllers(assemblies);
            HttpWebSocket.LoadControllers(assemblies);

            connectionWaitHandle = new AutoResetEvent(false);

            listenerLoopThread = new Thread(ListenerLoop)
            {
                Name = "HttpServer Listener",
                IsBackground = true
            };
            listenerLoopThread.Start();
        }

        ~HttpServer()
        {
            Stop();
        }

        public void Stop()
        {
            listenerLoopShutdown = true;
            connectionWaitHandle.Set();
            if (listenerLoopThread != null && listenerLoopThread.IsAlive)
            {
                listenerLoopThread.Join();
            }
        }

        #endregion

        #region properties

        public List<HttpConnection> Connections { get; private set; }
        public Dictionary<string, HttpSession> Sessions { get; private set; }
        public string Root => root;
        public int Port => port;
        public string FavIcon { get; set; }
        public HttpCache Cache { get; private set; }
        public object UserData { get; }

        #endregion

        #region service

        #region listners

        private Thread listenerLoopThread;
        private bool listenerLoopShutdown;

        private void ListenerLoop()
        {
            Thread.Sleep(300);
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            while (!listenerLoopShutdown)
            {
                try
                {
                    var result = listener.BeginAcceptTcpClient(AcceptTcpClientCallback, listener);
                    connectionWaitHandle.WaitOne();
                    connectionWaitHandle.Reset();
                }
                catch
                {
                    Thread.Sleep(5000);
                }
            }

            // shutdown listner
            if (listener != null)
            {
                listener.Stop();
            }

            listener = null;
        }

        private void AcceptTcpClientCallback(IAsyncResult ar)
        {
            try
            {
                var connection = (TcpListener) ar.AsyncState;
                var client = connection.EndAcceptTcpClient(ar);

                connectionWaitHandle.Set();

                lock (Connections)
                {
                    Connections.Add(new HttpConnection(this, client));
                    Connections.RemoveAll(c =>
                    {
                        if (c.Stopped)
                        {
                            c.Dispose();
                            return true;
                        }

                        return false;
                    });
                }
            }
            catch
            {
                connectionWaitHandle.Set();
            }
        }

        #endregion

        #endregion
    }
}