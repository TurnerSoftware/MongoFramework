using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using MongoFramework.Infrastructure.Serialization;

namespace MongoFramework.Benchmarks.Infrastructure.Serialization
{
	[CoreJob, MemoryDiagnoser]
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
