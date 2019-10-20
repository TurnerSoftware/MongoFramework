using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Infrastructure
{
	public class EntityRelationshipWriter<TEntity> : IEntityRelationshipWriter<TEntity> where TEntity : class
	{
		private IEnumerable<IEntityRelationship> Relationships { get; }

		private IMongoDbConnection Connection { get; }

		public EntityRelationshipWriter(IMongoDbConnection connection)
		{
			Connection = connection;
			Relationships = EntityMapping.GetOrCreateDefinition(typeof(TEntity)).Relationships;
		}

		public void CommitEntityRelationships(IEnumerable<TEntity> entities)
		{
			var writeMethod = GetType().GetMethod(nameof(CommitRelationship), BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (var relationship in Relationships)
			{
				writeMethod.MakeGenericMethod(relationship.EntityType)
					.Invoke(this, new object[] { relationship, entities });
			}
		}

		public async Task CommitEntityRelationshipsAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default(CancellationToken))
		{
			var writeMethod = GetType().GetMethod(nameof(CommitRelationshipAsync), BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (var relationship in Relationships)
			{
				await ((Task)writeMethod.MakeGenericMethod(relationship.EntityType)
					.Invoke(this, new object[] { relationship, entities, cancellationToken })).ConfigureAwait(false);
			}
		}

#pragma warning disable CRR0026 // Unused member - used via Reflection
		private void CommitRelationship<TRelatedEntity>(EntityRelationship relationship, IEnumerable<TEntity> entities) where TRelatedEntity : class
		{
			var collection = BuildRelatedEntityCollection<TRelatedEntity>(relationship, entities);
			if (collection.Any())
			{
				var dbSet = new MongoDbSet<TRelatedEntity>();
				dbSet.SetConnection(Connection);
				dbSet.AddRange(collection);
				dbSet.SaveChanges();
			}

			if (!relationship.IsCollection)
			{
				ApplyForeignKeyChanges<TRelatedEntity>(relationship, entities);
			}
		}
#pragma warning restore CRR0026 // Unused member - used via Reflection

#pragma warning disable CRR0026 // Unused member - used via Reflection
		private async Task CommitRelationshipAsync<TRelatedEntity>(EntityRelationship relationship, IEnumerable<TEntity> entities, CancellationToken cancellationToken) where TRelatedEntity : class
		{
			var collection = BuildRelatedEntityCollection<TRelatedEntity>(relationship, entities);

			cancellationToken.ThrowIfCancellationRequested();

			if (collection.Any())
			{
				var dbSet = new MongoDbSet<TRelatedEntity>();
				dbSet.SetConnection(Connection);
				dbSet.AddRange(collection);
				await dbSet.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
			}

			if (!relationship.IsCollection)
			{
				ApplyForeignKeyChanges<TRelatedEntity>(relationship, entities);
			}
		}
#pragma warning restore CRR0026 // Unused member - used via Reflection

		private IEntityCollection<TRelatedEntity> BuildRelatedEntityCollection<TRelatedEntity>(EntityRelationship relationship, IEnumerable<TEntity> entities) where TRelatedEntity : class
		{
			var definition = EntityMapping.GetOrCreateDefinition(typeof(TRelatedEntity));
			var collection = new EntityCollection<TRelatedEntity>();

			var defaultId = definition.GetDefaultId();
			var relatedEntitiesToProcess = new List<TRelatedEntity>();

			foreach (var entity in entities)
			{
				var propertyInfo = relationship.NavigationProperty.PropertyInfo;
				if (relationship.IsCollection)
				{
					if (propertyInfo.GetValue(entity) is ICollection<TRelatedEntity> navigationCollection)
					{
						relatedEntitiesToProcess.AddRange(navigationCollection);
					}
				}
				else
				{
					if (propertyInfo.GetValue(entity) is TRelatedEntity relatedEntity)
					{
						relatedEntitiesToProcess.Add(relatedEntity);
					}
				}
			}

			foreach (var relatedEntity in relatedEntitiesToProcess)
			{
				var entityId = definition.GetIdValue(relatedEntity);
				if (Equals(entityId, defaultId))
				{
					collection.Add(relatedEntity);
				}
			}

			return collection;
		}

		private void ApplyForeignKeyChanges<TRelatedEntity>(EntityRelationship relationship, IEnumerable<TEntity> entities) where TRelatedEntity : class
		{
			var definition = EntityMapping.GetOrCreateDefinition(typeof(TRelatedEntity));
			var defaultId = definition.GetDefaultId();

			foreach (var entity in entities)
			{
				var relatedEntity = (TRelatedEntity)relationship.NavigationProperty.GetValue(entity);
				var entityId = relatedEntity == null ? defaultId : definition.GetIdValue(relatedEntity);
				relationship.IdProperty.SetValue(entity, entityId);
			}
		}
	}
}
