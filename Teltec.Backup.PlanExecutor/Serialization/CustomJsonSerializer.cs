using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Reflection;

namespace Teltec.Backup.PlanExecutor.Serialization
{
	//
	// "How to serialize or deserialize a JSON Object to a certain depth in C#?" by "Nathan Baulch" is licensed under CC BY-SA 3.0
	//
	// Title?   How to serialize or deserialize a JSON Object to a certain depth in C#?
	// Author?  Nathan Baulch - http://stackoverflow.com/users/8799/nathan-baulch
	// Source?  http://stackoverflow.com/a/10454062/298054
	// License? CC BY-SA 3.0 - https://creativecommons.org/licenses/by-sa/3.0/legalcode
	//
	public static class CustomJsonSerializer
	{
		public static string SerializeObject(object obj, int maxDepth)
		{
			using (var strWriter = new StringWriter())
			{
				using (var jsonWriter = new CustomJsonTextWriter(strWriter))
				{
					Func<bool> include = () => jsonWriter.CurrentDepth <= maxDepth;
					var resolver = new CustomContractResolver(include);
					var serializer = new JsonSerializer { ContractResolver = resolver };
					serializer.Serialize(jsonWriter, obj);
				}
				return strWriter.ToString();
			}
		}

		public class CustomContractResolver : DefaultContractResolver
		{
			private readonly Func<bool> _includeProperty;

			public CustomContractResolver(Func<bool> includeProperty)
			{
				_includeProperty = includeProperty;
			}

			protected override JsonProperty CreateProperty(
				MemberInfo member, MemberSerialization memberSerialization)
			{
				var property = base.CreateProperty(member, memberSerialization);
				var shouldSerialize = property.ShouldSerialize;
				property.ShouldSerialize = obj => _includeProperty() &&
												  (shouldSerialize == null ||
												   shouldSerialize(obj));
				return property;
			}
		}

		public class CustomJsonTextWriter : JsonTextWriter
		{
			public CustomJsonTextWriter(TextWriter textWriter) : base(textWriter) { }

			public int CurrentDepth { get; private set; }

			public override void WriteStartObject()
			{
				CurrentDepth++;
				base.WriteStartObject();
			}

			public override void WriteEndObject()
			{
				CurrentDepth--;
				base.WriteEndObject();
			}
		}
	}
}
