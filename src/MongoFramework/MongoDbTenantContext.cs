using System.Collections.Generic;
using MongoFramework.Infrastructure.Commands;
using MongoFramework.Utilities;

namespace MongoFramework
{
	public class MongoDbTenantContext : MongoDbContext, IMongoDbTenantContext
	{
		public virtual string TenantId { get; }

		public MongoDbTenantContext(IMongoDbConnection connection, string tenantId) : base(connection)
		{
			Check.NotNull(tenantId, nameof(tenantId));
			TenantId = tenantId;
		}

		protected override void AfterDetectChanges()
		{
			ChangeTracker.EnforceMultiTenant(TenantId);
		}

		protected override WriteModelOptions GetWriteModelOptions()
		{
			return new WriteModelOptions { TenantId = TenantId };
		}

		public virtual void CheckEntity(IHaveTenantId entity)
		{
			Check.NotNull(entity, nameof(entity));

			if (entity.TenantId != TenantId)
			{
				throw new MultiTenantException($"Entity type {entity.GetType().Name}, tenant ID does not match. Expected: {TenantId}, Entity has: {entity.TenantId}");
			}
		}

		public virtual void CheckEntities(IEnumerable<IHaveTenantId> entities)
		{
			Check.NotNull(entities, nameof(entities));

			foreach (var entity in entities)
			{
				CheckEntity(entity);
			}
		}

		/// <summary>
		/// Marks the entity as unchanged in the change tracker and starts tracking.
		/// </summary>
		/// <param name="entity"></param>
		public override void Attach<TEntity>(TEntity entity) where TEntity : class
		{
			if (typeof(IHaveTenantId).IsAssignableFrom(typeof(TEntity)))
			{
				CheckEntity(entity as IHaveTenantId);
			}
			base.Attach(entity);
		}

		/// <summary>
		/// Marks the collection of entities as unchanged in the change tracker and starts tracking.
		/// </summary>
		/// <param name="entities"></param>
		public override void AttachRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
		{
			if (typeof(IHaveTenantId).IsAssignableFrom(typeof(TEntity)))
			{
				CheckEntities(entities as IEnumerable<IHaveTenantId>);
			}
			base.AttachRange(entities);
		}

	}
}
