using MongoDB.Driver;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Commands;
using MongoFramework.Infrastructure.Linq;
using MongoFramework.Infrastructure.Linq.Processors;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MongoFramework
{
	/// <summary>
	/// Basic Mongo "DbSet", providing entity tracking support.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public class MongoDbSet<TEntity> : IMongoDbSet<TEntity> where TEntity : class
	{
		public IMongoDbContext Context { get; }

		public MongoDbSet(IMongoDbContext context)
		{
			Context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public virtual TEntity Create()
		{
			var entity = Activator.CreateInstance<TEntity>();
			Add(entity);
			return entity;
		}

		/// <summary>
		///     Finds an entity with the given primary key value. If an entity with the given primary key value
		///     is being tracked by the context, then it is returned immediately without making a request to the
		///     database. Otherwise, a query is made to the database for an entity with the given primary key value
		///     and this entity, if found, is attached to the context and returned. If no entity is found, then
		///     null is returned.
		/// </summary>
		/// <param name="id">The value of the primary key for the entity to be found.</param>
		/// <returns>The entity found, or null.</returns>
		public virtual TEntity Find(object id) 
		{
			if (id == null)
				throw new ArgumentNullException(nameof(id));

			var tracked = Context.ChangeTracker.GetEntryById<TEntity>(id);

			if (tracked != null)
			{
				return tracked.Entity as TEntity;
			}

			var entityDefinition = EntityMapping.GetOrCreateDefinition(typeof(TEntity));
			var filter = entityDefinition.CreateIdFilter<TEntity>(id);

			var collection = Context.Connection.GetDatabase().GetCollection<TEntity>(entityDefinition.CollectionName);
			var cursor = collection.Find(filter);
			var entity = cursor.FirstOrDefault();

			if (entity != null)
			{
				Context.ChangeTracker.SetEntityState(entity, EntityEntryState.NoChanges);
			}

			return entity;
		}

		/// <summary>
		/// Marks the entity for insertion into the database.
		/// </summary>
		/// <param name="entity"></param>
		public virtual void Add(TEntity entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			Context.ChangeTracker.SetEntityState(entity, EntityEntryState.Added);
		}
		/// <summary>
		/// Marks the collection of entities for insertion into the database.
		/// </summary>
		/// <param name="entities"></param>
		public virtual void AddRange(IEnumerable<TEntity> entities)
		{
			if (entities == null)
			{
				throw new ArgumentNullException(nameof(entities));
			}

			foreach (var entity in entities)
			{
				Context.ChangeTracker.SetEntityState(entity, EntityEntryState.Added);
			}
		}

		/// <summary>
		/// Marks the entity for updating.
		/// </summary>
		/// <param name="entity"></param>
		public virtual void Update(TEntity entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			Context.ChangeTracker.SetEntityState(entity, EntityEntryState.Updated);
		}
		/// <summary>
		/// Marks the collection of entities for updating.
		/// </summary>
		/// <param name="entities"></param>
		public virtual void UpdateRange(IEnumerable<TEntity> entities)
		{
			if (entities == null)
			{
				throw new ArgumentNullException(nameof(entities));
			}

			foreach (var entity in entities)
			{
				Context.ChangeTracker.SetEntityState(entity, EntityEntryState.Updated);
			}
		}

		/// <summary>
		/// Marks the entity for deletion.
		/// </summary>
		/// <param name="entity"></param>
		public virtual void Remove(TEntity entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			Context.ChangeTracker.SetEntityState(entity, EntityEntryState.Deleted);
		}
		/// <summary>
		/// Marks the collection of entities for deletion.
		/// </summary>
		/// <param name="entities"></param>
		public virtual void RemoveRange(IEnumerable<TEntity> entities)
		{
			if (entities == null)
			{
				throw new ArgumentNullException(nameof(entities));
			}

			foreach (var entity in entities)
			{
				Context.ChangeTracker.SetEntityState(entity, EntityEntryState.Deleted);
			}
		}
		/// <summary>
		/// Stages a deletion for a range of entities that match the predicate
		/// </summary>
		/// <param name="targetField"></param>
		public virtual void RemoveRange(Expression<Func<TEntity, bool>> predicate)
		{
			Context.CommandStaging.Add(new RemoveEntityRangeCommand<TEntity>(predicate));
		}
		/// <summary>
		/// Stages a deletion for the entity that matches the specified ID
		/// </summary>
		/// <param name="entityId"></param>
		public virtual void RemoveById(object entityId)
		{
			Context.CommandStaging.Add(new RemoveEntityByIdCommand<TEntity>(entityId));
		}

		[Obsolete("Use SaveChanges on the IMongoDbContext")]
		public void SaveChanges()
		{
			Context.SaveChanges();
		}

		[Obsolete("Use SaveChangesAsync on the IMongoDbContext")]
		public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			await Context.SaveChangesAsync(cancellationToken);
		}

		#region IQueryable Implementation

		private IQueryable<TEntity> GetQueryable()
		{
			var queryable = Context.Query<TEntity>();
			var provider = queryable.Provider as IMongoFrameworkQueryProvider<TEntity>;
			provider.EntityProcessors.Add(new EntityTrackingProcessor<TEntity>(Context));
			return queryable;
		}

		public Expression Expression => GetQueryable().Expression;

		public Type ElementType => GetQueryable().ElementType;

		public IQueryProvider Provider => GetQueryable().Provider;

		public IEnumerator<TEntity> GetEnumerator()
		{
			return GetQueryable().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}