using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;

namespace MongoFramework.Infrastructure.Mapping;

public interface IEntityKeyGenerator
{
	public object Generate();
	public bool IsEmpty(object id);
}

public static class EntityKeyGenerators
{
	public static readonly IEntityKeyGenerator StringKeyGenerator = new EntityKeyGenerator(StringObjectIdGenerator.Instance);
	public static readonly IEntityKeyGenerator GuidKeyGenerator = new EntityKeyGenerator(CombGuidGenerator.Instance);
	public static readonly IEntityKeyGenerator ObjectIdKeyGenerator = new EntityKeyGenerator(ObjectIdGenerator.Instance);
}

internal sealed class EntityKeyGenerator : IEntityKeyGenerator
{
	private readonly IIdGenerator idGenerator;

	public EntityKeyGenerator(IIdGenerator idGenerator)
	{
		this.idGenerator = idGenerator;
	}

	public object Generate()
	{
		return idGenerator.GenerateId(null, null);
	}

	public bool IsEmpty(object id)
	{
		return idGenerator.IsEmpty(id);
	}
}

internal sealed class DriverKeyGeneratorWrapper : IIdGenerator
{
	private readonly IEntityKeyGenerator entityKeyGenerator;

	public DriverKeyGeneratorWrapper(IEntityKeyGenerator entityKeyGenerator)
	{
		this.entityKeyGenerator = entityKeyGenerator;
	}

	public object GenerateId(object container, object document) => entityKeyGenerator.Generate();

	public bool IsEmpty(object id) => entityKeyGenerator.IsEmpty(id);
}
