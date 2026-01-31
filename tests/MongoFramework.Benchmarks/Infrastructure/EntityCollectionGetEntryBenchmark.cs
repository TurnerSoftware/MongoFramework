using BenchmarkDotNet.Attributes;
using MongoFramework.Infrastructure;

namespace MongoFramework.Benchmarks.Infrastructure
{
	[MemoryDiagnoser]
	public class EntityCollectionGetEntryBenchmark
	{
		public class BenchmarkModel
		{
			public string Id { get; set; }
			public string Name { get; set; }
		}

		[Params(100, 1000, 10000)]
		public int NumberOfEntries { get; set; }

		private EntityEntryContainer container;
		private BenchmarkModel[] entities;

		[GlobalSetup]
		public void Setup()
		{
			container = new EntityEntryContainer();
			entities = new BenchmarkModel[NumberOfEntries];

			for (var i = 0; i < NumberOfEntries; i++)
			{
				entities[i] = new BenchmarkModel
				{
					Id = i.ToString(),
					Name = $"Entity {i}"
				};
			}
		}

		[Benchmark]
		public void SetEntityState()
		{
			for (var i = 0; i < NumberOfEntries; i++)
			{
				container.SetEntityState(entities[i], EntityEntryState.Added);
			}

			container.Clear();
		}
	}
}
