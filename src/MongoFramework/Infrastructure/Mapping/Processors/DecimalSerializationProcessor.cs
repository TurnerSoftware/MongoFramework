using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class DecimalSerializationProcessor : IMappingProcessor
	{
		private bool SerializerAdded { get; set; }

		public void ApplyMapping(IEntityDefinition definition)
		{
			if (!SerializerAdded)
			{
				SerializerAdded = true;
				BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));
				BsonSerializer.RegisterSerializer(typeof(decimal?), new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));
			}
		}
	}
}
