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
        public const int MAX_LIFE = 2;
        private readonly HttpServer server;

        private Dictionary<string, object> data;
        public string dataLock = "";


        public HttpSession(HttpServer server)
        {
            this.server = server;
            ID = Guid.NewGuid().ToString("D");
            CreationDate = DateTime.Now;
            LastRequest = DateTime.Now;
            data = new Dictionary<string, object>();

            expiryLoopThread = new Thread(ExpiryLoop)
            {
                Name = "HttpSession " + ID,
                IsBackground = true
            };
            expiryLoopThread.Start();
        }

        public string ID { get; }
        public DateTime CreationDate { get; }
        public DateTime LastRequest { get; set; }
        public DateTime ExpiryDate => LastRequest.AddDays(MAX_LIFE);

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

        ~HttpSession()
        {
            expiryLoopShutdown = true;

            if (expiryLoopThread != null && expiryLoopThread.IsAlive)
            {
                expiryLoopThread.Join();
            }
        }

        #region service

        private readonly Thread expiryLoopThread;
        private bool expiryLoopShutdown;

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
                        if (DateTime.Now > ExpiryDate)
                        {
                            // expired
                            data = new Dictionary<string, object>();

                            // throw an expired event
                            OnSessionExpiring(this);

                            // shutdown loop
                            expiryLoopShutdown = true;
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

        private SessionExpiredHandler sessionExpired;

        public event SessionExpiredHandler SessionExpired
        {
            add => sessionExpired += value;
            remove => sessionExpired -= value;
        }

        private void OnSessionExpiring(HttpSession session)
        {
            try
            {
                SessionExpiredHandler handler;
                if (null != (handler = sessionExpired))
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