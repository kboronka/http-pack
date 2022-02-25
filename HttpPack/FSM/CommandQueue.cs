using System;
using System.Collections.Generic;

namespace HttpPack.FSM;

/// <summary>
///     Description of CommandQueue.
/// </summary>
public class CommandQueue
{
    private readonly List<Command> commands;
    private readonly object queueLock = new();

    public CommandQueue()
    {
        lock (queueLock)
        {
            commands = new List<Command>();
        }
    }

    public bool Available => commands.Count > 0;

    public void QueueCommand(Enum command)
    {
        lock (queueLock)
        {
            commands.Add(new Command(command));
        }
    }

    public void QueueCommand(Enum command, params object[] paramerters)
    {
        lock (queueLock)
        {
            commands.Add(new Command(command, paramerters));
        }
    }

    public Command DequeueCommand()
    {
        lock (queueLock)
        {
            if (commands.Count == 0)
            {
                return null;
            }

            var currentCommand = commands[0];
            commands.RemoveAt(0);

            return currentCommand;
        }
    }
}