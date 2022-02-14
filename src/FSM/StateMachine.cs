using System.Threading;

namespace HttpPack.Fsm
{
    /// <summary>
    ///     Description of StateMachine.
    /// </summary>
    public abstract class StateMachine
    {
        public CommandQueue CommandQueue;
        protected bool loopStopped;
        protected bool loopStopRequested;

        protected Thread stateMachineThread;

        protected StateMachine()
        {
            CommandQueue = new CommandQueue();
            loopStopped = false;
            loopStopRequested = false;
        }

        public abstract void Start();
        public abstract void Stop();
    }
}