namespace MongoFramework
{
	public interface IMongoDbTenantContext : IMongoDbContext
	{
		string TenantId { get; }
	}
}
