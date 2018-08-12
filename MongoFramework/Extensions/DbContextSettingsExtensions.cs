using MongoDB.Driver;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Settings;

namespace MongoFramework
{
	public static class DbContextSettingsExtensions
	{
		public static IMongoDatabase GetDatabase(this IDbContextSettings settings)
		{
			return DatabaseProviderSettingsExtension.Extract(settings).GetDatabase();
		}

		public static IEntityMapper GetEntityMapper<TEntity>(this IDbContextSettings settings)
		{
			return EntityMappingProviderSettingsExtension.Extract(settings).GetEntityMapper(typeof(TEntity), settings);
		}
	}
}
