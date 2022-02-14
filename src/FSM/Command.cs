using System;

namespace HttpPack.Fsm
{
    /// <summary>
    ///     Description of Command.
    /// </summary>
    public class Command
    {
        public Command(Enum command, object[] parameters)
        {
            CommandSignal = command;
            Parameters = parameters;
        }

        public Command(Enum command)
            : this(command, null)
        {
        }

        public Enum CommandSignal { get; }
        public object[] Parameters { get; }
    }
}