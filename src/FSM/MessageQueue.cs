using System.Collections.Generic;
using System.Linq;

namespace HttpPack.Fsm
{
    /// <summary>
    ///     Description of MessageFIFO.
    /// </summary>
    public class MessageQueue<T>
    {
        private readonly object queueLock = new object();

        public MessageQueue()
        {
            lock (queueLock)
            {
                Messages = new List<Message<T>>();
            }
        }

        public bool Available
        {
            get { return Messages.Any(m => !m.Sent); }
        }

        public List<Message<T>> Messages { get; }

        public void QueueItem(T message)
        {
            lock (queueLock)
            {
                Messages.Add(new Message<T>(message));
            }
        }

        public void QueueItem(T message, Message<T>.MessageCallback responseCallback, int timeout,
            Message<T>.MessageExpiredCallback timeoutCallback)
        {
            lock (queueLock)
            {
                Messages.Add(new Message<T>(message, responseCallback, timeout, timeoutCallback));
            }
        }

        public T DequeueItem()
        {
            lock (queueLock)
            {
                foreach (var message in Messages)
                {
                    if (!message.Sent)
                    {
                        message.Sent = true;
                        return message.PayLoad;
                    }
                }
            }

            return default;
        }

        public void Cleanup()
        {
            lock (queueLock)
            {
                Messages.RemoveAll(m => m.Recived || m.Expired);
            }
        }
    }
}