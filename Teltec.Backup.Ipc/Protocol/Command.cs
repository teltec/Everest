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

		internal event CommandHandler Handler;
		public Command Parent { get; private set; }
		public int NumArguments { get; private set; }
		public int NumSubCommands { get; private set; }
		public IDictionary/*<string, ArgumentDefinition>*/ OrderedArgumentDefinitions { get; private set; }
		public List<Command> SubCommands { get; private set; }
		private bool _AllowAnonymous;
		private bool _HasTrailingArg;

		public bool RequiresAuth { get { return !_AllowAnonymous; } }
		public bool HasArguments { get { return NumArguments > 0; } }
		public bool HasSubCommands { get { return NumSubCommands > 0; } }

		public Command(string name)
		{
			Name = name;
		}

		public void InvokeHandler(object sender, EventArgs e)
		{
			if (Handler == null)
				return;

			Handler(sender, e);
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
		public Command WithArgument(string name, Type type, bool trailing = false)
		{
			if (HasSubCommands)
				throw new InvalidOperationException("A command cannot have both arguments and sub-commands");
			if (OrderedArgumentDefinitions == null)
				OrderedArgumentDefinitions = new OrderedDictionary/*<string, ArgumentDefinition>*/();
			if (_HasTrailingArg)
				throw new InvalidOperationException("A command cannot have any arguments after a trailing argument");

			if (trailing)
				_HasTrailingArg = true;

			//// Construct a `ArgumentDefinition<T>` using the passed `type` as `T`.
			//Type genericArgumentDefinitionType = typeof(ArgumentDefinition<>).MakeGenericType(type);
			//object argDef = Activator.CreateInstance(genericArgumentDefinitionType, name, trailing);

			ArgumentDefinition definition = new ArgumentDefinition(type, name, trailing);

			OrderedArgumentDefinitions.Add(name, definition);
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
