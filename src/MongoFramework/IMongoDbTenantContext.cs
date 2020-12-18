using System.Collections.Generic;

namespace MongoFramework
{
	public interface IMongoDbTenantContext : IMongoDbContext
	{
		string TenantId { get; }
		void CheckEntity(IHaveTenantId entity);
		void CheckEntities(IEnumerable<IHaveTenantId> entity);
	}
}
