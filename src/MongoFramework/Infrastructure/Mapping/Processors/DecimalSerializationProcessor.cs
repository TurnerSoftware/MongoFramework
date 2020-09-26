using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class DecimalSerializationProcessor : IMappingProcessor
	{
		private static bool ProviderAdded { get; set; }

		public void ApplyMapping(IEntityDefinition definition, BsonClassMap classMap)
		{
			if (!ProviderAdded)
			{
				ProviderAdded = true;
				BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));
				BsonSerializer.RegisterSerializer(typeof(decimal?), new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));
			}
		}
	}
}
