using MongoFramework.Infrastructure.Commands;
using System;

namespace MongoFramework
{
	public class MongoDbTenantContext : MongoDbContext, IMongoDbTenantContext
	{
		public virtual string TenantId { get; }

		public MongoDbTenantContext(IMongoDbConnection connection, string tenantId) : base(connection)
		{
			TenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId));
		}

		protected override void AfterDetectChanges()
		{
			ChangeTracker.EnforceMultiTenant(TenantId);
		}

		protected override WriteModelOptions GetWriteModelOptions()
		{
			return new WriteModelOptions { TenantId = TenantId };
		}
	}
}
