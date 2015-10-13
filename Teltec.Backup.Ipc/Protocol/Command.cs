using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Teltec.Backup.Ipc.Protocol
{
	public delegate void CommandHandler(object sender, EventArgs e);

	public class Command
	{
		public readonly string Name;

		internal CommandHandler Handler { get; set; }
		public Command Parent { get; private set; }
		public int NumArguments { get; private set; }
		public int NumSubCommands { get; private set; }
		public IDictionary/*<string, Type>*/ OrderedArgumentDefinitions { get; private set; }
		public List<Command> SubCommands { get; private set; }
		private bool _AllowAnonymous;

		public bool RequiresAuth { get { return !_AllowAnonymous; } }
		public bool HasArguments { get { return NumArguments > 0; } }
		public bool HasSubCommands { get { return NumSubCommands > 0; } }

		public Command(string name)
		{
			Name = name;
		}

		public bool AcceptsArgument(string argName)
		{
			return HasArguments && OrderedArgumentDefinitions.Contains(argName);
		}

		public Command AllowAnonymous()
		{
			_AllowAnonymous = true;
			return this;
		}

		// The `Type` must override `ToString` so the builder can actually build the command.
		public Command WithArgument(string name, Type type)
		{
			if (HasSubCommands)
				throw new InvalidOperationException("A command cannot have both arguments and sub-commands");
			if (OrderedArgumentDefinitions == null)
				OrderedArgumentDefinitions = new OrderedDictionary/*<string, Type>*/();
			OrderedArgumentDefinitions.Add(name, type);
			NumArguments++;
			return this;
		}

		public Command WithSubCommand(Command sub)
		{
			if (HasArguments)
				throw new InvalidOperationException("A command cannot have both arguments and sub-commands");
			if (SubCommands == null)
				SubCommands = new List<Command>();
			sub._AllowAnonymous = this._AllowAnonymous; // Inherit property.
			sub.Parent = this; // Store referencee to parent.
			SubCommands.Add(sub);
			NumSubCommands++;
			return this;
		}
	}
}
