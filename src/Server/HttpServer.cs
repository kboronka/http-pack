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
using System.Threading;
using System.Reflection;

using sar.Tools;

namespace sar.Http
{
    public class HttpServer : HttpBase
    {
        private AutoResetEvent connectionWaitHandle;
        private TcpListener listener;
        protected int port;
        protected string root;

        #region constructor

        public HttpServer(int port, string wwwroot, object userData)
        {
            this.UserData = userData;
            this.port = port;
            this.root = wwwroot;
            this.root = Path.GetFullPath(this.root);

            var assembly = GetAssembly();
            this.Start(GetReferencedAssemblies(assembly));
        }

        private void Start(List<Assembly> assemblies)
        {
            if (!Directory.Exists(this.root))
            {
                throw new DirectoryNotFoundException("root folder not found");
            }

            this.Connections = new List<HttpConnection>();
            this.Sessions = new Dictionary<string, HttpSession>();

            this.Cache = new HttpCache(this);
            HttpController.LoadControllers(assemblies);
            HttpWebSocket.LoadControllers(assemblies);

            this.connectionWaitHandle = new AutoResetEvent(false);

            this.listenerLoopThread = new Thread(this.ListenerLoop);
            this.listenerLoopThread.Name = "HttpServer Listener";
            this.listenerLoopThread.IsBackground = true;
            this.listenerLoopThread.Start();
        }

        ~HttpServer()
        {
            this.Stop();
        }

        public void Stop()
        {
            this.listenerLoopShutdown = true;
            connectionWaitHandle.Set();
            if (this.listenerLoopThread != null && this.listenerLoopThread.IsAlive)
            {
                this.listenerLoopThread.Join();
            }
        }

        #endregion

        #region properties

        public List<HttpConnection> Connections { get; private set; }
        public Dictionary<string, HttpSession> Sessions { get; private set; }
        public string Root { get { return root; } }
        public int Port { get { return port; } }
        public string FavIcon { get; set; }
        public HttpCache Cache { get; private set; }
        public Object UserData { get; private set; }

        #endregion

        #region service

        #region listners

        private Thread listenerLoopThread;
        private bool listenerLoopShutdown = false;

        private void ListenerLoop()
        {
            Thread.Sleep(300);
            this.listener = new TcpListener(IPAddress.Any, this.port);
            this.listener.Start();

            while (!listenerLoopShutdown)
            {
                try
                {
                    IAsyncResult result = this.listener.BeginAcceptTcpClient(this.AcceptTcpClientCallback, this.listener);
                    this.connectionWaitHandle.WaitOne();
                    this.connectionWaitHandle.Reset();
                }
                catch
                {
                    Thread.Sleep(5000);
                }
            }

            // shutdown listner
            if (this.listener != null)
            {
                this.listener.Stop();
            }

            this.listener = null;
        }

        private void AcceptTcpClientCallback(IAsyncResult ar)
        {
            try
            {
                var connection = (TcpListener)ar.AsyncState;
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
            var assemblies = new List<Assembly>();
            assemblies.Add(assembly);

            foreach (AssemblyName assemblyName in assembly.GetReferencedAssemblies())
            {
                string name = assemblyName.Name;

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
    }
}
