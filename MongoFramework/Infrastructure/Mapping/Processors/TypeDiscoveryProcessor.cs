using System;
using MongoDB.Bson.Serialization;
using System.Reflection;
using MongoFramework.Attributes;
using MongoFramework.Infrastructure.Mapping.Serialization;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class TypeDiscoveryProcessor : IMappingProcessor
	{
		private static bool ProviderAdded { get; set; }

		public void ApplyMapping(Type entityType, BsonClassMap classMap)
		{
			if (!ProviderAdded && entityType.GetCustomAttribute<RuntimeTypeDiscoveryAttribute>() != null)
			{
				ProviderAdded = true;
				BsonSerializer.RegisterSerializationProvider(TypeDiscoverySerializationProvider.Instance);
			}
		}
	}
}
