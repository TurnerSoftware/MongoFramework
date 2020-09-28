using MongoFramework.Infrastructure.Linq;
using MongoFramework.Infrastructure.Linq.Processors;
using MongoFramework.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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