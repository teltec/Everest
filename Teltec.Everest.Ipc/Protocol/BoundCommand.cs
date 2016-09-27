/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Teltec.Common.Extensions;
using Teltec.Everest.Ipc.Serialization;

namespace Teltec.Everest.Ipc.Protocol
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
			IsDirty = true;
		}

		public void InvokeHandler(object sender, EventArgs e)
		{
			Command.InvokeHandler(sender, e);
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

			Type argValueType = typeof(T);
			ArgumentDefinition argDefinition = (ArgumentDefinition)Command.OrderedArgumentDefinitions[argName];

			if (!argValueType.IsSameOrSubclass(argDefinition.Type))
			{
				string message = string.Format("Command {0} accepts argument {1} with type {2}, but received type {3}",
					Command.Name, argName, argDefinition.Type.Name, typeof(T).Name);
				throw new InvalidOperationException(message);
			}

			ArgumentValues.Add(argName, argValue);
			IsDirty = true;
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
				ArgumentDefinition acceptedArgDefinition = (ArgumentDefinition)entry.Value;
				string acceptedArgName = (string)entry.Key;
				Type acceptedArgType = acceptedArgDefinition.Type;
				// The call below DOES NOT CORRECTLY validate argument types.
				BindArgument(acceptedArgName, argValues[i++]);
			}

			return this;
		}

		public T GetArgumentValue<T>(string argName)
		{
			return (T)ArgumentValues[argName];
		}

		protected bool IsDirty;
		protected string BuiltCommand;

		protected void Build()
		{
			Stack<Command> commandStack = new Stack<Command>();
			// Stack commands until there is no parent.
			// For example: 1 RUN PLAN CONTROL
			for (Command cmd = this.Command; cmd != null; cmd = cmd.Parent)
			{
				commandStack.Push(cmd);
			}

			int numCommands = commandStack.Count;
			if (numCommands == 0)
			{
				BuiltCommand = null;
				IsDirty = false;
				return;
			}

			// Then unstack them and add to a list.
			// For example: CONTROL PLAN RUN 1
			List<string> commandList = new List<string>(numCommands);
			for (int i = 0; i < numCommands; i++)
			{
				Command cmd = commandStack.Pop();
				commandList.Add(cmd.Name);

				if (cmd.HasArguments)
				{
					foreach (DictionaryEntry acceptedArg in cmd.OrderedArgumentDefinitions)
					{
						ArgumentDefinition acceptedArgDefinition = (ArgumentDefinition)acceptedArg.Value;
						string acceptedArgName = (string)acceptedArg.Key;
						Type acceptedArgType = acceptedArgDefinition.Type;

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
								"Command {0} requires argument {1} of type {2}",
								cmd.Name, acceptedArgName, acceptedArgType.ToString()));
						}

						bool isComplex = passedArgType.IsSameOrSubclass(typeof(ComplexArgument));
						if (isComplex)
						{
							string serializedValue = NotNullJsonSerializer.SerializeObject(passedArgValue); // Needs strong typing?
							commandList.Add(serializedValue);
						}
						else
						{
							commandList.Add(passedArgValue.ToString());
						}
					}

					break; // Ignore remaining commands.
				}
			}

			// Then transform it into a valid command string.
			BuiltCommand = string.Join(" ", commandList);
			IsDirty = false;
		}

		public override string ToString()
		{
			if (IsDirty)
				Build();

			return BuiltCommand;
		}
	}
}
