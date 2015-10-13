using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Teltec.Backup.Ipc.Protocol
{
	public class CommandParser
	{
		private readonly Command[] AcceptedCommands;

		public CommandParser(Command[] acceptedCommands)
		{
			AcceptedCommands = acceptedCommands;
		}

		public BoundCommand ParseMessage(Message msg, out string errorMessage)
		{
			return ParseMessage(msg, out errorMessage, AcceptedCommands, null);
		}

		private static BoundCommand ParseMessage(Message msg, out string errorMessage, Command[] acceptedCommands, BoundCommand lastCommand = null)
		{
			string currentToken = msg.NextToken();
			if (currentToken == null)
			{
				errorMessage = lastCommand == null ? "Invalid command" : null;
				return lastCommand;
			}

			Command commandMatch = Array.Find(acceptedCommands, delegate(Command cmd)
			{
				return currentToken.Equals(cmd.Name, StringComparison.InvariantCulture);
			});

			if (commandMatch == null)
			{
				errorMessage = string.Format("Unknown command: {0}", currentToken);
				return null;
			}

			BoundCommand boundCommand = new BoundCommand(commandMatch);

			if (commandMatch.HasSubCommands)
			{
				// Find sub-command.
				boundCommand = ParseMessage(msg, out errorMessage, commandMatch.SubCommands.ToArray(), boundCommand);
			}
			else if (commandMatch.HasArguments)
			{
				LinkedList<string> argValues = new LinkedList<string>();

				// Read and validate arguments.
				foreach (DictionaryEntry entry in commandMatch.OrderedArgumentDefinitions)
				{
					currentToken = msg.NextToken();

					string definedArgName = (string)entry.Key;
					Type definedArgType = (Type)entry.Value;
					string passedArgValue = currentToken;

					try
					{
						object deserializedValue = JsonConvert.DeserializeObject(passedArgValue, definedArgType);
						boundCommand.BindArgument(definedArgName, deserializedValue);
					}
					catch (Exception ex)
					{
						errorMessage = ex.Message;
						return null;
					}
				}
			}

			errorMessage = null;
			return boundCommand;
		}
	}
}
