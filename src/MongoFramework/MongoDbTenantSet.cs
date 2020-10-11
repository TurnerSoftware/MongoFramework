using MongoFramework.Infrastructure.Linq;
using MongoFramework.Infrastructure.Linq.Processors;
using MongoFramework.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Commands;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Utilities;

namespace MongoFramework
{
	/// <summary>
	/// Basic Mongo "DbSet", providing entity tracking support and multi-tenancy.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public class MongoDbTenantSet<TEntity> : MongoDbSet<TEntity>, IMongoDbTenantSet<TEntity> where TEntity : class, IHaveTenantId
	{
		public new IMongoDbTenantContext Context { get; }
		IMongoDbContext IMongoDbSet<TEntity>.Context => Context;

		public MongoDbTenantSet(IMongoDbContext context) : base(context)
		{
			Context = context as IMongoDbTenantContext ?? throw new ArgumentException("Context provided to a MongoDbTenantSet must be IMongoDbTenantContext",nameof(context));
		}

		protected virtual void CheckEntity(TEntity entity)
		{
			Check.NotNull(entity, nameof(entity));
			
			if (entity.TenantId != Context.TenantId)
			{
				throw new MultiTenantException($"Entity type {entity.GetType().Name}, tenant ID does not match. Expected: {Context.TenantId}, Entity has: {entity.TenantId}");
			}
		}

		protected virtual void CheckEntities(IEnumerable<TEntity> entities)
		{
			Check.NotNull(entities, nameof(entities));
			
			foreach (var entity in entities)
			{
				CheckEntity(entity);
			}
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
		public override TEntity Find(object id)
		{
			Check.NotNull(id, nameof(id));

			var tracked = Context.ChangeTracker.GetEntryById<TEntity>(id);

			if (tracked != null)
			{
				if (tracked.Entity is IHaveTenantId tenantEntity)
				{
					if (tenantEntity.TenantId == Context.TenantId)
					{
						return tracked.Entity as TEntity;
					}
				}
				else
				{
					return tracked.Entity as TEntity;
				}
			}

			var entityDefinition = EntityMapping.GetOrCreateDefinition(typeof(TEntity));
			var filter = entityDefinition.CreateIdFilter<TEntity>(id, Context.TenantId);

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
		///     Finds an entity with the given primary key value. If an entity with the given primary key value
		///     is being tracked by the context, then it is returned immediately without making a request to the
		///     database. Otherwise, a query is made to the database for an entity with the given primary key value
		///     and this entity, if found, is attached to the context and returned. If no entity is found, then
		///     null is returned.
		/// </summary>
		/// <param name="id">The value of the primary key for the entity to be found.</param>
		/// <returns>The entity found, or null.</returns>
		public override async ValueTask<TEntity> FindAsync(object id)
		{
			Check.NotNull(id, nameof(id));

			var tracked = Context.ChangeTracker.GetEntryById<TEntity>(id);

			if (tracked != null)
			{
				return tracked.Entity as TEntity;
			}

			var entityDefinition = EntityMapping.GetOrCreateDefinition(typeof(TEntity));
			var filter = entityDefinition.CreateIdFilter<TEntity>(id, Context.TenantId);

			var collection = Context.Connection.GetDatabase().GetCollection<TEntity>(entityDefinition.CollectionName);
			var cursor = await collection.FindAsync(filter);
			var entity = await cursor.FirstOrDefaultAsync();

			if (entity != null)
			{
				Context.ChangeTracker.SetEntityState(entity, EntityEntryState.NoChanges);
			}

			return entity;
		}

		public override void Add(TEntity entity)
		{
			Check.NotNull(entity, nameof(entity));

			entity.TenantId = Context.TenantId;
			base.Add(entity);
		}

		public override void AddRange(IEnumerable<TEntity> entities)
		{
			Check.NotNull(entities, nameof(entities));

			foreach (var entity in entities)
			{
				entity.TenantId = Context.TenantId;
			}
			base.AddRange(entities);
		}

		public override void Update(TEntity entity)
		{
			CheckEntity(entity);
			base.Update(entity);
		}

		public override void UpdateRange(IEnumerable<TEntity> entities)
		{
			CheckEntities(entities);
			base.UpdateRange(entities);
		}

		public override void Remove(TEntity entity)
		{
			CheckEntity(entity);
			base.Remove(entity);
		}

		public override void RemoveRange(IEnumerable<TEntity> entities)
		{
			CheckEntities(entities);
			base.RemoveRange(entities);
		}

		public override void RemoveRange(Expression<Func<TEntity, bool>> predicate)
		{
			var key = Context.TenantId;
			var filter = predicate.AndAlso(o => o.TenantId == key);
			base.RemoveRange(filter);
		}
		
		#region IQueryable Implementation

		protected override IQueryable<TEntity> GetQueryable()
		{
			var key = Context.TenantId;
			var queryable = Context.Query<TEntity>().Where(c => c.TenantId == key);
			var provider = queryable.Provider as IMongoFrameworkQueryProvider<TEntity>;
			provider.EntityProcessors.Add(new EntityTrackingProcessor<TEntity>(Context));
			return queryable;
		}
		
		public IQueryable<TEntity> GetSearchTextQueryable(string search)
		{			
			var key = Context.TenantId;
			var queryable = Context.Query<TEntity>().WhereFilter(b => b.Text(search)).Where(c => c.TenantId == key);
			var provider = queryable.Provider as IMongoFrameworkQueryProvider<TEntity>;
			provider.EntityProcessors.Add(new EntityTrackingProcessor<TEntity>(Context));
			return queryable;
		}

		#endregion
	}
}