using System.Threading;
using HttpPack.Json;

namespace HttpPack.FiniteStateMachine;

/// <summary>
///     Description of Message.
/// </summary>
public class Message<T>
{
    public delegate void MessageCallback(JsonKeyValuePairs kvp);

    public delegate void MessageExpiredCallback(T payload);

    private readonly object recivedLock = new();
    private readonly int timeout;

    private readonly Thread timeoutThread;

    public Message(T payload)
    {
        PayLoad = payload;
    }

    public Message(T payload, MessageCallback responseCallback, int timeout, MessageExpiredCallback timeoutCallback)
        : this(payload)
    {
        ResponseCallback = responseCallback;
        TimeoutCallback = timeoutCallback;
        this.timeout = timeout;

        timeoutThread = new Thread(TimeoutLoop)
        {
            Name = "Message Timeout Thread",
            IsBackground = true,
            Priority = ThreadPriority.Lowest
        };
        timeoutThread.Start();
    }

    public bool Sent { get; set; }
    public bool Recived { get; set; }
    public bool Expired { get; private set; }

    public T PayLoad { get; set; }

    public MessageCallback ResponseCallback { get; }
    public MessageExpiredCallback TimeoutCallback { get; }

    public void RequestSent()
    {
        Sent = true;
    }

    public void ResponseRecived(JsonKeyValuePairs kvp)
    {
        try
        {
            Monitor.Enter(recivedLock);

            Recived = true;

            if (!Expired && ResponseCallback != null)
            {
                ResponseCallback(kvp);
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

                        if (!Recived && TimeoutCallback != null)
                        {
                            TimeoutCallback(PayLoad);
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