using System;
using System.Collections.Generic;
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoFramework.Attributes;
using MongoFramework.Utilities;

namespace MongoFramework.Infrastructure.Serialization
{
	public class TypeDiscoverySerializationProvider : BsonSerializationProviderBase
	{
		public static TypeDiscoverySerializationProvider Instance { get; } = new TypeDiscoverySerializationProvider();

		public bool Enabled { get; set; } = true;

		public override IBsonSerializer GetSerializer(Type type, IBsonSerializerRegistry serializerRegistry)
		{
			Check.NotNull(type, nameof(type));

			if (Enabled && (type.GetCustomAttribute<RuntimeTypeDiscoveryAttribute>() != null || type == typeof(object)))
			{
				var serializerType = typeof(TypeDiscoverySerializer<>).MakeGenericType(type);
				return (IBsonSerializer)Activator.CreateInstance(serializerType);
			}

			return null;
		}
	}
}
