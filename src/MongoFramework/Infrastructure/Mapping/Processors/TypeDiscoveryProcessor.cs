using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Serialization;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class TypeDiscoveryProcessor : IMappingProcessor
	{
		private bool ProviderAdded { get; set; }

		public void ApplyMapping(IEntityDefinition definition)
		{
			if (!ProviderAdded)
			{
				ProviderAdded = true;
				BsonSerializer.RegisterSerializationProvider(TypeDiscoverySerializationProvider.Instance);
			}
		}
	}
}
