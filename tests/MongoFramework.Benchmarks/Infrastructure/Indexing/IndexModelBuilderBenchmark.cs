using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MongoFramework.Attributes;
using MongoFramework.Infrastructure.Indexing;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Benchmarks.Infrastructure.Indexing
{
	[SimpleJob(RuntimeMoniker.Net50), MemoryDiagnoser]
	public class IndexModelBuilderBenchmark
	{
		public class FlatIndexModel
		{
			[Index(IndexSortOrder.Ascending)]
			public string NoNameIndex { get; set; }
			[Index("MyCustomIndexName", IndexSortOrder.Ascending)]
			public string NamedIndex { get; set; }

			[Index("MyCompoundIndex", IndexSortOrder.Ascending, IndexPriority = 1)]
			public string FirstPriority { get; set; }

			[Index("MyCompoundIndex", IndexSortOrder.Ascending, IndexPriority = 3)]
			public string ThirdPriority { get; set; }

			[Index("MyCompoundIndex", IndexSortOrder.Ascending, IndexPriority = 2)]
			public string SecondPriority { get; set; }
		}

		public class NestedIndexParentModel
		{
			[Index(IndexSortOrder.Ascending)]
			public string NoNameIndex { get; set; }
			[Index("MyCustomIndexName", IndexSortOrder.Ascending)]
			public string NamedIndex { get; set; }
			public IEnumerable<NestedIndexChildModel> ChildEnumerable { get; set; }
			public NestedIndexChildModel[] ChildArray { get; set; }
			public List<NestedIndexChildModel> ChildList { get; set; }
		}
		public class NestedIndexChildModel
		{
			[Index(IndexSortOrder.Ascending)]
			public string ChildId { get; set; }
		}

		[GlobalSetup]
		public void Setup()
		{
			EntityMapping.RemoveAllDefinitions();
			MongoDbDriverHelper.ResetDriver();
			EntityMapping.RegisterType(typeof(FlatIndexModel));
			EntityMapping.RegisterType(typeof(NestedIndexParentModel));
		}

		[Benchmark]
		public void FlatModel()
		{
			IndexModelBuilder<FlatIndexModel>.BuildModel().ToArray();
		}

		[Benchmark]
		public void NestedModel()
		{
			IndexModelBuilder<NestedIndexParentModel>.BuildModel().ToArray();
		}
	}
}
