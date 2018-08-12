using MongoFramework.Infrastructure.Indexing;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.Linq;

namespace MongoFramework.Infrastructure.Settings
{
	public abstract class EntityMappingProviderSettingsExtension : IDbContextSettingsExtension
	{
		public abstract IEntityMapper GetEntityMapper(Type entityType, IDbContextSettings settings);
		public abstract IEntityIndexMapper GetEntityIndexMapper(Type entityType, IDbContextSettings settings);

		public static EntityMappingProviderSettingsExtension Extract(IDbContextSettings settings)
		{
			var providerExtensions = settings.Extensions.OfType<EntityMappingProviderSettingsExtension>().ToList();

			if (providerExtensions.Count == 0)
			{
				throw new InvalidOperationException($"No entity mapping provider configured");
			}

			if (providerExtensions.Count > 1)
			{
				throw new InvalidOperationException($"Only one entity mapping provider can be configured");
			}

			return providerExtensions[0];
		}
	}
}
