using System;
using System.Collections.Generic;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework;

public class MappingBuilder
{
	private readonly IEnumerable<IMappingProcessor> mappingConventions;
	private readonly List<EntityDefinitionBuilder> builders = new();

	public IReadOnlyList<EntityDefinitionBuilder> Definitions => builders;

	public MappingBuilder(IEnumerable<IMappingProcessor> mappingProcessors)
	{
		mappingConventions = mappingProcessors;
	}

	private bool TryGetBuilder(Type entityType, out EntityDefinitionBuilder builder)
	{
		builder = builders.Find(b => b.EntityType == entityType);
		return builder != null;
	}

	private void UpdateBuilder(Type entityType, EntityDefinitionBuilder builder)
	{
		var index = builders.FindIndex(b => b.EntityType == entityType);
		builders[index] = builder;
	}

	private void ApplyMappingConventions(EntityDefinitionBuilder definitionBuilder)
	{
		foreach (var processor in mappingConventions)
		{
			processor.ApplyMapping(definitionBuilder);
		}
	}

	public EntityDefinitionBuilder Entity(Type entityType)
	{
		if (!TryGetBuilder(entityType, out var definitionBuilder))
		{
			definitionBuilder = new EntityDefinitionBuilder(entityType, this);
			builders.Add(definitionBuilder);
			ApplyMappingConventions(definitionBuilder);
		}

		return definitionBuilder;
	}

	public EntityDefinitionBuilder<TEntity> Entity<TEntity>()
	{
		if (!TryGetBuilder(typeof(TEntity), out var definitionBuilder))
		{
			definitionBuilder = new EntityDefinitionBuilder<TEntity>(this);
			builders.Add(definitionBuilder);
			ApplyMappingConventions(definitionBuilder);
		}

		//Allow upgrading from non-generic entity definition
		if (definitionBuilder is not EntityDefinitionBuilder<TEntity>)
		{
			definitionBuilder = EntityDefinitionBuilder<TEntity>.CreateFrom(definitionBuilder);
			UpdateBuilder(typeof(TEntity), definitionBuilder);
		}

		return definitionBuilder as EntityDefinitionBuilder<TEntity>;
	}
}
