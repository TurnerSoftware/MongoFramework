using System;
using System.Collections.Generic;
using System.Linq;
using MongoFramework.Infrastructure;

namespace MongoFramework
{
	public class DbContextSettings : IDbContextSettings
	{
		private readonly IReadOnlyDictionary<Type, IDbContextSettingsExtension> InternalExtensions;

		public IEnumerable<IDbContextSettingsExtension> Extensions => InternalExtensions.Values;

		public DbContextSettings() : this(new Dictionary<Type, IDbContextSettingsExtension>()) { }
		public DbContextSettings(IReadOnlyDictionary<Type, IDbContextSettingsExtension> extensions)
		{
			InternalExtensions = extensions;
		}

		public TExtension FindExtension<TExtension>() where TExtension : class, IDbContextSettingsExtension
		{
			return InternalExtensions.TryGetValue(typeof(TExtension), out var extension) ? (TExtension)extension : null;
		}

		public TExtension GetExtension<TExtension>() where TExtension : class, IDbContextSettingsExtension
		{
			var extension = FindExtension<TExtension>();
			if (extension == null)
			{
				throw new InvalidOperationException($"Extension {typeof(TExtension).Name} can not be found in the settings");
			}
			return extension;
		}

		public IDbContextSettings WithExtension<TExtension>(TExtension extension) where TExtension : class, IDbContextSettingsExtension
		{
			var extensions = Extensions.ToDictionary(e => e.GetType(), e => e);
			extensions[typeof(TExtension)] = extension;
			return new DbContextSettings(extensions);
		}
	}
}
