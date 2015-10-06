using System;
using System.Collections;
using System.Collections.Generic;
using Teltec.Common.Extensions;

namespace Teltec.Backup.Ipc.Protocol
{
	public class BoundCommandEventArgs : EventArgs
	{
		public BoundCommand Command;
	}

	public class BoundCommand
	{
		public readonly Command Command;
		public readonly Dictionary<string, object> ArgumentValues;
		public bool HasArguments { get { return Command.HasArguments; } }
		public bool RequiresAuth { get { return Command.RequiresAuth; } }

		public BoundCommand(Command command)
		{
			Command = command;
			if (Command.HasArguments)
				ArgumentValues = new Dictionary<string, object>(Command.NumArguments);
		}

		public void InvokeHandler(object sender, EventArgs e)
		{
			if (Command.Handler == null)
				return;

			Command.Handler.Invoke(sender, e);
		}

		public BoundCommand BindArgument<T>(string argName, T argValue)
		{
			if (!HasArguments)
			{
				string message = string.Format("Command {0} does not accept any arguments", Command.Name);
				throw new InvalidOperationException(message);
			}
			if (!Command.AcceptsArgument(argName))
			{
				string message = string.Format("Command {0} does not accept argument {1}", Command.Name, argName);
				throw new InvalidOperationException(message);
			}

			Type argType = (Type)Command.OrderedArgumentDefinitions[argName];

			if (!typeof(T).IsSameOrSubclass(argType))
			{
				string message = string.Format("Command {0} accepts argument {1} with type {2}, but received type {3}",
					Command.Name, argName, argType.Name, typeof(T).Name);
				throw new InvalidOperationException(message);
			}

			ArgumentValues.Add(argName, argValue);
			return this;
		}

		public BoundCommand BindArguments(Dictionary<string /* argName */, object /* argValue */> arguments)
		{
			foreach (var arg in arguments)
			{
				BindArgument(arg.Key, arg.Value);
			}
			return this;
		}

		public BoundCommand BindArguments(params KeyValuePair<string /* argName */, object /* argValue */>[] arguments)
		{
			foreach (var arg in arguments)
			{
				BindArgument(arg.Key, arg.Value);
			}
			return this;
		}

		public BoundCommand BindArguments(object[] argValues)
		{
			int numProvidedArgs = argValues != null ? argValues.Length : 0;
			int numRequiredArgs = Command.NumArguments;

			if (numProvidedArgs != numRequiredArgs)
			{
				throw new InvalidOperationException(string.Format(
					"Command {0} requires {1} arguments, but only {2} were provided",
					Command.Name, numRequiredArgs, numProvidedArgs));
			}

			int i = 0;
			foreach (DictionaryEntry entry in Command.OrderedArgumentDefinitions)
			{
				string acceptedArgName = (string)entry.Key;
				Type acceptedArgType = (Type)entry.Value;
				BindArgument(acceptedArgName, argValues[i++]);
			}

			return this;
		}

		public T GetArgumentValue<T>(string argName)
		{
			return (T)ArgumentValues[argName];
		}

		public override string ToString()
		{
			Queue<Command> commandQueue = new Queue<Command>();
			// Enqueue commands until there is no parent.
			// For example: 1 RUN PLAN CONTROL
			for (Command cmd = this.Command; cmd != null; cmd = cmd.Parent)
			{
				commandQueue.Enqueue(cmd);
			}

			// Then dequeue them and add to a list.
			// For example: CONTROL PLAN RUN 1
			List<string> commandList = new List<string>(commandQueue.Count);
			for (Command cmd = commandQueue.Dequeue(); cmd != null; cmd = commandQueue.Dequeue())
			{
				commandList.Add(cmd.Name);

				if (cmd.HasArguments)
				{
					foreach (DictionaryEntry acceptedArg in cmd.OrderedArgumentDefinitions)
					{
						string acceptedArgName = (string)acceptedArg.Key;
						Type acceptedArgType = (Type)acceptedArg.Value;

						bool found = ArgumentValues.ContainsKey(acceptedArgName);
						if (!found)
						{
							throw new InvalidOperationException(string.Format(
								"Command {0} requires argument {1}", cmd.Name, acceptedArgName));
						}

						object passedArgValue = ArgumentValues[acceptedArgName];
						Type passedArgType = passedArgValue.GetType();
						if (!passedArgType.IsSameOrSubclass(acceptedArgType))
						{
							throw new InvalidOperationException(string.Format(
								"Command {0} requires argument {0} of type {1}",
								cmd.Name, acceptedArgName, acceptedArgType.ToString()));
						}

						commandList.Add(passedArgValue.ToString());
					}

					break; // Ignore remaining commands.
				}
			}

			// Then transform it into a valid command string.
			return string.Join(" ", commandList);
		}
	}
}
