using MongoDB.Driver;
using MongoFramework.Infrastructure.Mapping;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.EntityRelationships
{
	public class EntityRelationshipWriter<TEntity> : IEntityRelationshipWriter<TEntity>
	{
		private IEnumerable<EntityRelationship> Relationships { get; }

		private IDbContextSettings Settings { get; set; }

		public EntityRelationshipWriter(IDbContextSettings settings)
		{
			Settings = settings;

			var entityMapper = settings.GetEntityMapper<TEntity>();
			Relationships = entityMapper.GetEntityRelationships();
		}

		public void CommitEntityRelationships(IEnumerable<TEntity> entities)
		{
			var writeMethod = GetType().GetMethod("CommitRelationship", BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (var relationship in Relationships)
			{
				writeMethod.MakeGenericMethod(relationship.EntityType)
					.Invoke(this, new object[] { relationship, entities });
			}
		}

		public async Task CommitEntityRelationshipsAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
		{
			var writeMethod = GetType().GetMethod("CommitRelationshipAsync", BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (var relationship in Relationships)
			{
				await ((Task)writeMethod.MakeGenericMethod(relationship.EntityType)
					.Invoke(this, new object[] { relationship, entities, cancellationToken })).ConfigureAwait(false);
			}
		}

#pragma warning disable CRR0026 // Unused member - used via Reflection
		private void CommitRelationship<TRelatedEntity>(EntityRelationship relationship, IEnumerable<TEntity> entities)
		{
			var collection = BuildRelatedEntityCollection<TRelatedEntity>(relationship, entities);
			if (collection.Any())
			{
				var dbSet = new MongoDbSet<TRelatedEntity>(Settings);
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
		private async Task CommitRelationshipAsync<TRelatedEntity>(EntityRelationship relationship, IEnumerable<TEntity> entities, CancellationToken cancellationToken)
		{
			var collection = BuildRelatedEntityCollection<TRelatedEntity>(relationship, entities);

			cancellationToken.ThrowIfCancellationRequested();

			if (collection.Any())
			{
				var dbSet = new MongoDbSet<TRelatedEntity>(Settings);
				dbSet.AddRange(collection);
				await dbSet.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
			}

			if (!relationship.IsCollection)
			{
				ApplyForeignKeyChanges<TRelatedEntity>(relationship, entities);
			}
		}
#pragma warning restore CRR0026 // Unused member - used via Reflection

		private IDbEntityCollection<TRelatedEntity> BuildRelatedEntityCollection<TRelatedEntity>(EntityRelationship relationship, IEnumerable<TEntity> entities)
		{
			var collection = new DbEntityCollection<TRelatedEntity>();
			var entityMapper = new EntityMapper<TRelatedEntity>();

			var defaultId = entityMapper.GetDefaultId();
			var relatedEntitiesToProcess = new List<TRelatedEntity>();

			foreach (var entity in entities)
			{
				if (relationship.IsCollection)
				{
					if (relationship.NavigationProperty.GetValue(entity) is ICollection<TRelatedEntity> navigationCollection)
					{
						relatedEntitiesToProcess.AddRange(navigationCollection);
					}
				}
				else
				{
					var relatedEntity = (TRelatedEntity)relationship.NavigationProperty.GetValue(entity);
					if (relatedEntity != null)
					{
						relatedEntitiesToProcess.Add(relatedEntity);
					}
				}
			}

			foreach (var relatedEntity in relatedEntitiesToProcess)
			{
				var entityId = entityMapper.GetIdValue(relatedEntity);
				if (Equals(entityId, defaultId))
				{
					collection.Add(relatedEntity);
				}
			}

			return collection;
		}

		private void ApplyForeignKeyChanges<TRelatedEntity>(EntityRelationship relationship, IEnumerable<TEntity> entities)
		{
			var entityMapper = new EntityMapper<TRelatedEntity>();
			var defaultId = entityMapper.GetDefaultId();

			foreach (var entity in entities)
			{
				var relatedEntity = (TRelatedEntity)relationship.NavigationProperty.GetValue(entity);
				var entityId = relatedEntity == null ? defaultId : entityMapper.GetIdValue(relatedEntity);
				relationship.IdProperty.SetValue(entity, entityId);
			}
		}
	}
}
