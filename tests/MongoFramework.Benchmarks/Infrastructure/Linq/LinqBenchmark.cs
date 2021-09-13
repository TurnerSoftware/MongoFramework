using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MongoFramework.Infrastructure.Linq;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Benchmarks.Infrastructure.Linq
{

	[SimpleJob(RuntimeMoniker.Net50), MemoryDiagnoser]
	public class LinqBenchmark
	{
		private IMongoDbConnection Connection { get; set; }

		public class TestModel
		{
			public string Id { get; set; }
		}

		[GlobalSetup]
		public void Setup()
		{
			EntityMapping.TryRegisterType(typeof(TestModel), out _);
			Connection = BenchmarkDb.GetConnection();

			//Pre-JIT the benchmarks to avoid issues with benchmark
			FirstOrDefault();
			ToArray();
			Count();
			Any();
		}

		private IMongoFrameworkQueryable<TestModel> GetQueryable()
		{
			var provider = new MongoFrameworkQueryProvider<TestModel>(Connection);
			return new MongoFrameworkQueryable<TestModel>(provider);
		}

		[Benchmark]
		public TestModel FirstOrDefault()
		{
			return GetQueryable().FirstOrDefault();
		}

		[Benchmark]
		public TestModel[] ToArray()
		{
			return GetQueryable().ToArray();
		}

		[Benchmark]
		public int Count()
		{
			return GetQueryable().Count();
		}

		[Benchmark]
		public int Any()
		{
			return GetQueryable().Count();
		}
	}
}
