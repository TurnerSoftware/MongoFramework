using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Infrastructure.Commands
{
	public static class EntityDefinitionExtensions
	{
		public static FilterDefinition<TEntity> CreateIdFilterFromEntity<TEntity>(this IEntityDefinition definition, TEntity entity)
		{
			return Builders<TEntity>.Filter.Eq(definition.GetIdName(), definition.GetIdValue(entity));
		}
		public static FilterDefinition<TEntity> CreateIdFilter<TEntity>(this IEntityDefinition definition, object entityId)
		{
			return Builders<TEntity>.Filter.Eq(definition.GetIdName(), entityId);
		}
	}
}
