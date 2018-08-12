using System.Collections.Generic;

namespace MongoFramework.Infrastructure
{
	public interface IDbContextSettings
	{
		IEnumerable<IDbContextSettingsExtension> Extensions { get; }
		TExtension FindExtension<TExtension>() where TExtension : class, IDbContextSettingsExtension;
		TExtension GetExtension<TExtension>() where TExtension : class, IDbContextSettingsExtension;
		IDbContextSettings WithExtension<TExtension>(TExtension extension) where TExtension : class, IDbContextSettingsExtension;
	}
}
