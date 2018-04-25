using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using MongoDB.Bson.Serialization;
using MongoFramework.Attributes;

namespace MongoFramework.Infrastructure.Mapping.Serialization
{
	public class TypeDiscoverySerializationProvider : BsonSerializationProviderBase
	{
		public static TypeDiscoverySerializationProvider Instance { get; } = new TypeDiscoverySerializationProvider();

		public bool Enabled { get; set; } = true;

		public override IBsonSerializer GetSerializer(Type type, IBsonSerializerRegistry serializerRegistry)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (Enabled && type.GetCustomAttribute<RuntimeTypeDiscoveryAttribute>() != null)
			{
				var serializerType = typeof(TypeDiscoverySerializer<>).MakeGenericType(type);
				return (IBsonSerializer)Activator.CreateInstance(serializerType);
			}

			return null;
		}
	}
}
