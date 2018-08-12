using MongoDB.Driver;

namespace MongoFramework.Infrastructure.Settings
{
	public class DefaultDatabaseProviderSettingsExtension : DatabaseProviderSettingsExtension
	{
		public override IMongoDatabase GetDatabase()
		{
			var mongoUrl = new MongoUrl(ConnectionString);
			var client = new MongoClient(mongoUrl);
			return client.GetDatabase(mongoUrl.DatabaseName, DatabaseSettings);
		}
	}
}
