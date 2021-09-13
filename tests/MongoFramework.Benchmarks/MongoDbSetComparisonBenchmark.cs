using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MongoFramework.Attributes;

namespace MongoFramework.Benchmarks
{
	[SimpleJob(RuntimeMoniker.Net50), MemoryDiagnoser]
	public class MongoDbSetComparisonBenchmark
	{
		private class TestModel
		{
			public string Id { get; set; }
			[Index(IndexSortOrder.Ascending)]
			public string Name { get; set; }
			public DateTime Date { get; set; }
			public int OtherReference { get; set; }
			public NestedTestModel NestedModel { get; set; }
		}

		private class NestedTestModel
		{
			public string NestedName { get; set; }
			public int NestedReference { get; set; }
		}

		private class TestBucketGroup
		{
			[Index(IndexSortOrder.Ascending)]
			public string Name { get; set; }
		}

		private class TestBucketItem
		{
			public DateTime Date { get; set; }
			public int OtherReference { get; set; }
			public NestedTestModel NestedModel { get; set; }
		}

		private class CustomDbContext : MongoDbContext
		{
			public CustomDbContext(IMongoDbConnection connection) : base(connection) { }
			public MongoDbSet<TestModel> TestModels { get; set; }
			[BucketSetOptions(200, nameof(TestBucketItem.Date))]
			public MongoDbBucketSet<TestBucketGroup, TestBucketItem> TestBucketModels { get; set; }
		}

		public int NumberOfGroups = 8;
		public int EntitiesPerGroup = 200;

		[GlobalSetup]
		public void Setup()
		{
			BenchmarkDb.DropDatabase();
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			BenchmarkDb.DropDatabase();
		}

		[Benchmark]
		public void MongoDbSet()
		{
			var connection = BenchmarkDb.GetConnection();
			var date = new DateTime(2000, 1, 1);
			using (var context = new CustomDbContext(connection))
			{
				//Add
				for (var i = 0; i < NumberOfGroups; i++)
				{
					for (var j = 0; j < EntitiesPerGroup; j++)
					{
						var v = i * EntitiesPerGroup + j;
						context.TestModels.Add(new TestModel
						{
							Date = date.AddDays(v),
							Name = "TestModel",
							OtherReference = i * EntitiesPerGroup + v,
							NestedModel = new NestedTestModel
							{
								NestedName = $"TestModel {v}",
								NestedReference = v
							}
						});
					}
					context.SaveChanges();
				}

				//Read
				for (var i = 0; i < EntitiesPerGroup; i++)
				{
					context.TestModels
						.Where(t => t.Name == "TestModel")
						.Sum(t => t.OtherReference);
				}

				//Remove
				context.TestModels.RemoveRange(t => true);
				context.SaveChanges();
			}
		}

		[Benchmark]
		public void MongoDbBucketSet()
		{
			var connection = BenchmarkDb.GetConnection();
			var date = new DateTime(2000, 1, 1);
			using (var context = new CustomDbContext(connection))
			{
				//Add
				for (var i = 0; i < NumberOfGroups; i++)
				{
					for (var j = 0; j < EntitiesPerGroup; j++)
					{
						var v = i * EntitiesPerGroup + j;
						context.TestBucketModels.Add(new TestBucketGroup
						{
							Name = "TestModel"
						},
						new TestBucketItem
						{
							Date = date.AddDays(v),
							OtherReference = v,
							NestedModel = new NestedTestModel
							{
								NestedName = $"TestModel {v}",
								NestedReference = v
							}
						});
					}
					context.SaveChanges();
				}

				//Read
				for (var i = 0; i < EntitiesPerGroup; i++)
				{
					context.TestBucketModels.WithGroup(new TestBucketGroup
					{
						Name = "TestModel"
					}).Sum(t => t.OtherReference);
				}

				//Remove
				context.TestBucketModels.Remove(new TestBucketGroup
				{
					Name = "TestModel"
				});
				context.SaveChanges();
			}
		}
	}
}
