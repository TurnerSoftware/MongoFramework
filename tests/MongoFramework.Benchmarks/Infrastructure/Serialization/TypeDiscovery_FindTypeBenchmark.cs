using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MongoFramework.Infrastructure.Serialization;

namespace MongoFramework.Benchmarks.Infrastructure.Serialization
{
	[SimpleJob(RuntimeMoniker.Net50), MemoryDiagnoser]
	public class TypeDiscovery_FindTypeBenchmark
	{
		private class LocalClass { }

		[Benchmark]
		public void Overhead()
		{
			TypeDiscovery.ClearCache();
		}

		[Benchmark]
		public void FindString()
		{
			TypeDiscovery.FindTypeByDiscriminator("string", typeof(object));
			TypeDiscovery.ClearCache();
		}

		[Benchmark]
		public void FindMongoDbContext()
		{
			TypeDiscovery.FindTypeByDiscriminator("MongoDbContext", typeof(object));
			TypeDiscovery.ClearCache();
		}

		[Benchmark]
		public void FindLocalClass()
		{
			TypeDiscovery.FindTypeByDiscriminator("LocalClass", typeof(object));
			TypeDiscovery.ClearCache();
		}
	}
}
