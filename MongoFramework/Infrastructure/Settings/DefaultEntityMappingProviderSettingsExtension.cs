using MongoFramework.Infrastructure.Indexing;
using MongoFramework.Infrastructure.Mapping;
using System;

namespace MongoFramework.Infrastructure.Settings
{
	public class DefaultEntityMappingProviderSettingsExtension : EntityMappingProviderSettingsExtension
	{
		public override IEntityIndexMapper GetEntityIndexMapper(Type entityType, IDbContextSettings settings)
		{
			return new EntityIndexMapper(GetEntityMapper(entityType, settings));
		}

		public override IEntityMapper GetEntityMapper(Type entityType, IDbContextSettings settings)
		{
			return new EntityMapper(entityType, settings);
		}
	}
}
