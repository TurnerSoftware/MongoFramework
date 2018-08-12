using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoFramework.Infrastructure;

namespace MongoFramework
{
	public class DbContextSettingsBuilder
	{
		public IDbContextSettings Settings { get; private set; }

		public DbContextSettingsBuilder(IDbContextSettings settings)
		{
			Settings = settings ?? throw new ArgumentNullException(nameof(settings));
		}

		public bool IsConfigured => Settings.Extensions.Any();

		public void AddOrUpdateExtension<TExtension>(TExtension extension) where TExtension : class, IDbContextSettingsExtension
		{
			Settings = Settings.WithExtension(extension);
		}
	}
}
