using MongoDB.Bson.Serialization;

namespace MongoFramework.Infrastructure.Mapping
{
	internal class EntityKeyGenerator : IEntityKeyGenerator
	{
		private IIdGenerator IdGenerator { get; }

		public EntityKeyGenerator(IIdGenerator idGenerator)
		{
			IdGenerator = idGenerator;
		}

		public object Generate()
		{
			return IdGenerator.GenerateId(null, null);
		}
	}
}
