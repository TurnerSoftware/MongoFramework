using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;

namespace MongoFramework.Infrastructure.Mapping;

public interface IEntityKeyGenerator
{
	public object Generate();
	public bool IsEmpty(object id);
}

public class EntityKeyGenerators
{
	public static readonly IEntityKeyGenerator StringKeyGenerator = new EntityKeyGenerator(StringObjectIdGenerator.Instance);
	public static readonly IEntityKeyGenerator GuidKeyGenerator = new EntityKeyGenerator(CombGuidGenerator.Instance);
	public static readonly IEntityKeyGenerator ObjectIdKeyGenerator = new EntityKeyGenerator(ObjectIdGenerator.Instance);
}

internal class EntityKeyGenerator : IEntityKeyGenerator
{
	internal IIdGenerator IdGenerator { get; }

	public EntityKeyGenerator(IIdGenerator idGenerator)
	{
		IdGenerator = idGenerator;
	}

	public object Generate()
	{
		return IdGenerator.GenerateId(null, null);
	}

	public bool IsEmpty(object id)
	{
		return IdGenerator.IsEmpty(id);
	}
}

internal class DriverKeyGeneratorWrapper : IIdGenerator
{
	private readonly IEntityKeyGenerator entityKeyGenerator;

	public DriverKeyGeneratorWrapper(IEntityKeyGenerator entityKeyGenerator)
	{
		this.entityKeyGenerator = entityKeyGenerator;
	}

	public object GenerateId(object container, object document) => entityKeyGenerator.Generate();

	public bool IsEmpty(object id) => entityKeyGenerator.IsEmpty(id);
}
