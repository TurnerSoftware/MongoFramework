using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Commands;
using MongoFramework.Infrastructure.Indexing;
using MongoFramework.Infrastructure.Internal;
using MongoFramework.Infrastructure.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MongoFramework
{
	public class MongoDbTenantContext : MongoDbContext, IMongoDbTenantContext, IDisposable
	{
		public virtual string TenantId { get; protected set; }

		public MongoDbTenantContext(IMongoDbConnection connection, string tenantId) : base(connection)
		{
			TenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId));
		}

		public override void AfterDetectChanges()
		{
			ChangeTracker.EnforceMultiTenant(TenantId);
		}
	}
}
