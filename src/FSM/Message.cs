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
using System.Linq;
using System.Threading;

using HttpPack.Json;
using HttpPack.Fsm;

namespace HttpPack.Fsm
{
	/// <summary>
	/// Description of Message.
	/// </summary>
	public class Message<T>
	{
		private readonly int timeout;
		private readonly object recivedLock = new object();
		
		public bool Sent { get; set; }
		public bool Recived { get; set; }
		
		private readonly Thread timeoutThread;
		public bool Expired { get; private set; }

		public T PayLoad { get; set; }

		public delegate void MessageCallback(JsonKeyValuePairs kvp);
		public delegate void MessageExpiredCallback(T payload);

		public MessageCallback ResponseCallback { get; private set; }
		public MessageExpiredCallback TimeoutCallback { get; private set; }
		
		public Message(T payload)
		{
			this.PayLoad = payload;
		}
		
		public Message(T payload, MessageCallback responseCallback, int timeout, MessageExpiredCallback timeoutCallback)
			: this(payload)
		{
			this.ResponseCallback = responseCallback;
			this.TimeoutCallback = timeoutCallback;
			this.timeout = timeout;

            timeoutThread = new Thread(TimeoutLoop)
            {
                Name = "Message Timeout Thread",
                IsBackground = true,
                Priority = ThreadPriority.Lowest
            };
            timeoutThread.Start();
		}
		
		public void RequestSent()
		{
			this.Sent = true;
		}
		
		public void ResponseRecived(JsonKeyValuePairs kvp)
		{
			try
			{
				Monitor.Enter(recivedLock);

				this.Recived = true;
				
				if (!this.Expired && this.ResponseCallback != null)
				{
					this.ResponseCallback(kvp);
				}
			}
			finally
			{
				Monitor.Exit(recivedLock);
			}
		}
		
		private void TimeoutLoop()
		{
			var timeoutTimer = new Interval(timeout);
			
			while (!Expired)
			{
				Thread.Sleep(200);
				
				if (Monitor.TryEnter(recivedLock, 500))
				{
					try
					{
						if (timeoutTimer.Ready)
						{
							Expired = true;
							
							if (!Recived && this.TimeoutCallback != null)
							{
								this.TimeoutCallback(PayLoad);
							}
						}
					}
					finally
					{
						Monitor.Exit(recivedLock);
					}
				}
				else
				{
					// The lock was not acquired.
					return;
				}
			}
		}
	}
}
