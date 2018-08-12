using Microsoft.Extensions.DependencyInjection;

namespace MongoFramework.Infrastructure
{
	public interface IDbContextSettingsExtension
	{
		void ApplyServices(IServiceCollection serviceCollection);
	}
}
