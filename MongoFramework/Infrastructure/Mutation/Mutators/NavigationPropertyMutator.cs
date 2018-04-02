using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Mapping;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;
using MongoFramework.Infrastructure.EntityRelationships;

namespace MongoFramework.Infrastructure.Mutation.Mutators
{
	public class NavigationPropertyMutator<TEntity> : IEntityMutator<TEntity>
	{
		public void MutateEntity(TEntity entity, MutatorType mutationType, IEntityMapper entityMapper, IMongoDatabase database)
		{
			if (database == null)
			{
				throw new ArgumentNullException(nameof(database));
			}

			if (mutationType == MutatorType.Select)
			{
				ProcessRead(entity, entityMapper, database);
			}
			else
			{
				ProcessWrite(entity, entityMapper, database);
			}
		}

		private void ProcessRead(TEntity entity, IEntityMapper entityMapper, IMongoDatabase database)
		{
			var relationships = EntityRelationshipHelper.GetRelationshipsForType(typeof(TEntity));

			foreach (var relationship in relationships)
			{
				if (relationship.IsCollection)
				{
					var collection = relationship.NavigationProperty.GetValue(entity) as IEntityNavigationCollection;
					collection.FinaliseImport(database);
				}
				else
				{

				}
			}

			var completeMapping = entityMapper.TraverseMapping().ToArray();



			foreach (var mapping in completeMapping)
			{
				var foreignKeyName = mapping.Property.Name;
				var foreignKeyAttr = mapping.Property.GetCustomAttribute<ForeignKeyAttribute>();

				if (foreignKeyName.EndsWith("Id") || foreignKeyAttr != null)
				{
					var navigationPropertyName = foreignKeyAttr?.Name ?? foreignKeyName.Substring(0, foreignKeyName.Length - 2);

					//if (!propertyMap.ContainsKey(navigationPropertyName))
					//{
					//	throw new MongoFrameworkMappingException($"Can't find property ${navigationPropertyName} on ${entityType.Name} for navigation property mapping.");
					//}

					//var navigationProperty = propertyMap[navigationPropertyName];
					//classMap.UnmapMember(navigationProperty);
				}
			}
		}

		private void ProcessWrite(TEntity entity, IEntityMapper entityMapper, IMongoDatabase database)
		{
			var completeMapping = entityMapper.TraverseMapping().ToArray();
		}
	}
}
