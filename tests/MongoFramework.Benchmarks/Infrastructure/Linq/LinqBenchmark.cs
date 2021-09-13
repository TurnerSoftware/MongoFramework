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
			Where_OfType_OrderBy_Select_FirstOrDefault();
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
		public bool Any()
		{
			return GetQueryable().Any();
		}

		[Benchmark]
		public object Where_OfType_OrderBy_Select_FirstOrDefault()
		{
			return GetQueryable().Where(e => e.Id == "123").OfType<TestModel>().OrderBy(e => e.Id).Select(e => new { A = e.Id, B = e.Id }).FirstOrDefault();
		}
	}
}
