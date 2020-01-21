using System;
using MongoDB.Bson.Serialization;
using System.Reflection;
using MongoFramework.Attributes;
using MongoFramework.Infrastructure.Serialization;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class TypeDiscoveryProcessor : IMappingProcessor
	{
		private static bool ProviderAdded { get; set; }
		public void ApplyMapping(IEntityDefinition definition, BsonClassMap classMap)
		{
			if (!ProviderAdded)
			{
				ProviderAdded = true;
				BsonSerializer.RegisterSerializationProvider(TypeDiscoverySerializationProvider.Instance);
			}
		}
	}
}
