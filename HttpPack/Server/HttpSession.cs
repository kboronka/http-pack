using System;
using System.Collections.Generic;
using System.Threading;
using HttpPack.FiniteStateMachine;

namespace HttpPack.Server;

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