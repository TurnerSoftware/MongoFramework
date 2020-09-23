using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Commands;
using MongoFramework.Infrastructure.Linq;
using MongoFramework.Infrastructure.Linq.Processors;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Linq;
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
	public class MongoDbTenantSet<TEntity> : IMongoDbTenantSet<TEntity> where TEntity : class, IHaveTenantId
	{
		public IMongoDbTenantContext Context { get; }
		IMongoDbContext IMongoDbSet<TEntity>.Context => Context;

		public MongoDbTenantSet(IMongoDbContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}
			Context = (context as IMongoDbTenantContext) ?? throw new ArgumentException("Context provided to a MongoDbTenantSet must be IMongoDbTenantContext",nameof(context));
		}

		public virtual TEntity Create()
		{
			var entity = Activator.CreateInstance<TEntity>();
			entity.TenantId = Context.TenantId;			
			Add(entity);
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
			entity.TenantId = Context.TenantId;
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
				entity.TenantId = Context.TenantId;
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

			if (entity.TenantId != Context.TenantId)
			{
				throw new MultiTenantException($"Entity type {entity.GetType().Name}, tenant ID does not match. Expected: {Context.TenantId}, entity has: {entity.TenantId}");
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
				if (entity.TenantId != Context.TenantId)
				{
					throw new MultiTenantException($"Entity type {entity.GetType().Name}, tenant ID does not match. Expected: {Context.TenantId}, entity has: {entity.TenantId}");
				}
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
			if (entity.TenantId != Context.TenantId)
			{
				throw new MultiTenantException($"Entity type {entity.GetType().Name}, tenant ID does not match. Expected: {Context.TenantId}, entity has: {entity.TenantId}");
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
				if (entity.TenantId != Context.TenantId)
				{
					throw new MultiTenantException($"Entity type {entity.GetType().Name}, tenant ID does not match. Expected: {Context.TenantId}, entity has: {entity.TenantId}");
				}
				Context.ChangeTracker.SetEntityState(entity, EntityEntryState.Deleted);
			}
		}
		/// <summary>
		/// Stages a deletion for a range of entities that match the predicate
		/// </summary>
		/// <param name="targetField"></param>
		public virtual void RemoveRange(Expression<Func<TEntity, bool>> predicate)
		{
			var key = Context.TenantId;
			var filter = predicate.AndAlso(o => o.TenantId == key);
			Context.CommandStaging.Add(new RemoveEntityRangeCommand<TEntity>(filter));
		}
		/// <summary>
		/// Stages a deletion for the entity that matches the specified ID
		/// </summary>
		/// <param name="entityId"></param>
		public virtual void RemoveById(object entityId)
		{
			Context.CommandStaging.Add(new RemoveEntityByIdCommand<TEntity>(entityId, Context.TenantId));
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
			var key = Context.TenantId;
			var queryable = Context.Query<TEntity>().Where(c => c.TenantId == key);
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