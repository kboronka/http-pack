using System;
using System.Diagnostics;

namespace HttpPack.FiniteStateMachine;

public class Interval
{
    private readonly long setPoint;
    private readonly Stopwatch time;
    private long lastTrigger;
    private long pauseStartTime;
    private long pauseTime;

    public void Reset()
    {
        lock (time)
        {
            lastTrigger += setPoint + pauseTime;
            pauseTime = 0;

            var timeToNextTrigger = time.ElapsedMilliseconds - lastTrigger;

            if (timeToNextTrigger > setPoint || timeToNextTrigger < 0)
            {
                lastTrigger = time.ElapsedMilliseconds;
            }
        }
    }

    public void Pause()
    {
        lock (time)
        {
            if (!Paused)
            {
                Paused = true;
                pauseStartTime = time.ElapsedMilliseconds;
            }
        }
    }

    public void Continue()
    {
        lock (time)
        {
            if (Paused)
            {
                Paused = false;
                pauseTime += time.ElapsedMilliseconds - pauseStartTime;
            }
        }
    }

    #region properties

    public long Clock
    {
        get
        {
            lock (time)
            {
                return time.ElapsedMilliseconds;
            }
        }
    }

    public long ElapsedMilliseconds
    {
        get
        {
            var pauseTime = PausedTime;

            lock (time)
            {
                return time.ElapsedMilliseconds - lastTrigger - pauseTime;
            }
        }
    }

    /// <summary>
    ///     Returns number of milliseconds remianing before interval is ready
    /// </summary>
    public long Remaining => SetPoint - Math.Min(ElapsedMilliseconds, SetPoint);

    /// <summary>
    ///     Returns a percentage (0 - 100).  100% = Ready.
    /// </summary>
    public double PercentComplete
    {
        get
        {
            var elapsedTime = (double) Math.Min(ElapsedMilliseconds, SetPoint);
            return elapsedTime / SetPoint * 100.0;
        }
    }

    public long SetPoint
    {
        get
        {
            lock (time)
            {
                return setPoint;
            }
        }
    }

    public bool Ready
    {
        get
        {
            if (ElapsedMilliseconds >= setPoint)
            {
                Reset();
                return true;
            }

            return false;
        }
    }

    public bool Paused { get; private set; }

    /// <summary>
    ///     Returns number of milliseconds interval has been paused
    /// </summary>
    public long PausedTime
    {
        get
        {
            lock (time)
            {
                if (Paused)
                {
                    return pauseTime + (time.ElapsedMilliseconds - pauseStartTime);
                }

                return pauseTime;
            }
        }
    }

    #endregion

    #region constructor

    public Interval(long setPoint, long firstRunDelay)
    {
        time = new Stopwatch();
        time.Start();
        this.setPoint = setPoint;
        lastTrigger = time.ElapsedMilliseconds - setPoint + firstRunDelay;
        Paused = false;
        pauseTime = 0;
    }

    public Interval(long setPoint) : this(setPoint, setPoint)
    {
    }

    #endregion
}