/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Teltec.Common.Extensions;

namespace Teltec.Everest.Ipc.Protocol
{
	[Serializable]
	public abstract class ComplexArgument
	{
	}

	public class ArgumentDefinition
	{
		public Type Type { get; set; }
		public string Name { get; set; }
		public bool Trailing { get; set; }
		public bool IsComplex
		{
			get { return Type.IsSameOrSubclass(typeof(ComplexArgument)); }
		}

		public ArgumentDefinition(Type type, string name, bool trailing)
		{
			Type = type;
			Name = name;
			Trailing = trailing;
		}
	}

	//public class ArgumentValue<T>
	//{
	//	public Type Type { get; set; }
	//	public string Name { get; set; }
	//	public T Value { get; set; }
	//
	//	public ArgumentValue(string name, T value)
	//	{
	//		Type = typeof(T);
	//		Name = name;
	//		Value = value;
	//	}
	//}
}
