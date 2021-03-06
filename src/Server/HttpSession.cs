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
using System.Threading;

using HttpPack.Fsm;

namespace HttpPack
{
    public class HttpSession
    {
        public string dataLock = "";


        public const int MAX_LIFE = 2;

        private Dictionary<string, object> data;
        private readonly HttpServer server;



        public HttpSession(HttpServer server)
        {
            this.server = server;
            this.ID = Guid.NewGuid().ToString("D");
            this.CreationDate = DateTime.Now;
            this.LastRequest = DateTime.Now;
            this.data = new Dictionary<string, object>();

            this.expiryLoopThread = new Thread(this.ExpiryLoop)
            {
                Name = "HttpSession " + this.ID,
                IsBackground = true
            };
            this.expiryLoopThread.Start();
        }

        ~HttpSession()
        {
            this.expiryLoopShutdown = true;

            if (this.expiryLoopThread != null && this.expiryLoopThread.IsAlive)
            {
                this.expiryLoopThread.Join();
            }

        }

        public string ID { get; private set; }
        public DateTime CreationDate { get; private set; }
        public DateTime LastRequest { get; set; }
        public DateTime ExpiryDate { get { return LastRequest.AddDays(MAX_LIFE); } }

        public Dictionary<string, object> Data
        {
            get
            {
                lock (dataLock)
                {
                    return data;
                }
            }
        }

        #region service

        private readonly Thread expiryLoopThread;
        private bool expiryLoopShutdown = false;

        private void ExpiryLoop()
        {
            // every thirty minutes
            var expiryCheck = new Interval(30 * 60000, 5000);

            while (!expiryLoopShutdown)
            {
                try
                {
                    if (expiryCheck.Ready)
                    {
                        if (DateTime.Now > this.ExpiryDate)
                        {
                            // expired
                            this.data = new Dictionary<string, object>();

                            // throw an expired event
                            OnSessionExpiring(this);

                            // shutdown loop
                            this.expiryLoopShutdown = true;
                        }
                    }

                    Thread.Sleep(1000);
                }
                catch
                {
                    Thread.Sleep(2000);
                }
            }
        }

        #endregion

        #region events

        public delegate void SessionExpiredHandler(HttpSession session);

        #region session expired

        private SessionExpiredHandler sessionExpired = null;
        public event SessionExpiredHandler SessionExpired
        {
            add
            {
                this.sessionExpired += value;
            }
            remove
            {
                this.sessionExpired -= value;
            }
        }

        private void OnSessionExpiring(HttpSession session)
        {
            try
            {
                SessionExpiredHandler handler;
                if (null != (handler = (SessionExpiredHandler)this.sessionExpired))
                {
                    handler(session);
                }
            }
            catch
            {
                
            }
        }

        #endregion

        #endregion

    }
}
