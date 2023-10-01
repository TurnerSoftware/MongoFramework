using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MongoFramework.Infrastructure;

namespace MongoFramework.Benchmarks;

[SimpleJob(RuntimeMoniker.Net60), MemoryDiagnoser]
public class EntityEntryContainerBenchmark
{
	[Params(100, 1_000, 10_000)]
	public int EntryCount;

	private EntityEntryContainer Container;
	private TestEntity[] Entities;

	private class TestEntity
	{
		public string Id { get; set; }
	}

	[GlobalSetup]
	public void Setup()
	{
		Container = new EntityEntryContainer();
		Entities = new TestEntity[EntryCount];
		for (var i = 0; i < Entities.Length; i++)
		{
			Entities[i] = new TestEntity();
		}
	}

	[Benchmark]
	public void SetEntityState()
	{
		for (var i = 0; i < Entities.Length; i++)
		{
			Container.SetEntityState(Entities[i], EntityEntryState.Added);
		}
	}
}
