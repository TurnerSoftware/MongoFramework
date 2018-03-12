using MongoFramework.Tests.Models;

namespace MongoFramework.Tests
{
	public class MongoDbContextTestContext : MongoDbContext
	{
		public MongoDbContextTestContext(string connectionString, string databaseName) : base(connectionString, databaseName) { }

		public MongoDbSet<MongoDbContextModel> ContextDbSet { get; set; }
	}
}