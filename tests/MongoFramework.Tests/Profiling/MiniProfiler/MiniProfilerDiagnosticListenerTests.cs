using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Attributes;
using MongoFramework.Profiling.MiniProfiler;
using SEProfiling = StackExchange.Profiling;

namespace MongoFramework.Tests.Profiling.MiniProfiler
{
	[TestClass]
	public class MiniProfilerDiagnosticListenerTests : TestBase
	{
		private IMongoDbConnection GetConnection()
		{
			var connection = TestConfiguration.GetConnection();
			connection.DiagnosticListener = new MiniProfilerDiagnosticListener();
			return connection;
		}

		class TestContext : MongoDbContext
		{
			public TestContext(IMongoDbConnection connection) : base(connection) { }

			public MongoDbSet<GeneralProfileModel> GeneralProfiling { get; set; }
			public MongoDbSet<ProfileIndexModel> IndexProfiling { get; set; }
		}

		class GeneralProfileModel
		{
			public string Id { get; set; }
			public string Name { get; set; }
			public int Number { get; set; }
		}

		class ProfileIndexModel
		{
			public string Id { get; set; }
			[Index("TestIndex", IndexSortOrder.Ascending)]
			public string IndexSpecificDescriptionField { get; set; }
		}

		[TestMethod]
		public void ProfilingInsert()
		{
			using (var context = new TestContext(GetConnection()))
			{
				var profiler = SEProfiling.MiniProfiler.StartNew();
				context.GeneralProfiling.Add(new GeneralProfileModel
				{
					Name = "ProfilingInsert",
				});
				context.SaveChanges();

				Assert.IsTrue(profiler.Root.CustomTimings.ContainsKey("mongodb"));
				var timings = profiler.Root.CustomTimings["mongodb"];
				Assert.IsTrue(timings[0].CommandString.Contains("InsertOne"));
				Assert.IsTrue(timings[0].CommandString.Contains("ProfilingInsert"));
			}
		}

		[TestMethod]
		public void ProfilingUpdate()
		{
			using (var context = new TestContext(GetConnection()))
			{
				var entity = new GeneralProfileModel
				{
					Name = "ProfilingUpdate",
				};
				context.GeneralProfiling.Add(entity);
				context.SaveChanges();

				var profiler = SEProfiling.MiniProfiler.StartNew();

				entity.Name = "ProfilingUpdate-Updated";
				context.SaveChanges();

				Assert.IsTrue(profiler.Root.CustomTimings.ContainsKey("mongodb"));
				var timings = profiler.Root.CustomTimings["mongodb"];
				Assert.IsTrue(timings[0].CommandString.Contains("UpdateOne"));
				Assert.IsTrue(timings[0].CommandString.Contains("ProfilingUpdate-Updated"));
			}
		}

		[TestMethod]
		public void ProfilingDelete()
		{
			using (var context = new TestContext(GetConnection()))
			{
				var entity = new GeneralProfileModel
				{
					Name = "ProfilingDelete",
				};
				context.GeneralProfiling.Add(entity);
				context.SaveChanges();

				var profiler = SEProfiling.MiniProfiler.StartNew();

				context.GeneralProfiling.Remove(entity);
				context.SaveChanges();

				Assert.IsTrue(profiler.Root.CustomTimings.ContainsKey("mongodb"));
				var timings = profiler.Root.CustomTimings["mongodb"];
				Assert.IsTrue(timings[0].CommandString.Contains("DeleteOne"));
				Assert.IsTrue(timings[0].CommandString.Contains(entity.Id));
			}
		}

		[TestMethod]
		public void ProfilingRead()
		{
			using (var context = new TestContext(GetConnection()))
			{
				for (int i = 0, l = 100; i < l; i++)
				{
					context.GeneralProfiling.Add(new GeneralProfileModel
					{
						Number = i,
						Name = "ProfilingRead"
					});
				}

				context.SaveChanges();

				var profiler = SEProfiling.MiniProfiler.StartNew();

				var queryable = context.GeneralProfiling.Skip(78);
				foreach (var entity in queryable)
				{ }

				Assert.IsTrue(profiler.Root.CustomTimings.ContainsKey("mongodb"));
				var timings = profiler.Root.CustomTimings["mongodb"];
				Assert.IsTrue(timings[0].CommandString.Contains("$skip"));
				Assert.IsTrue(timings[0].CommandString.Contains("78"));
			}
		}

		[TestMethod]
		public void ProfilingReadWithEnforcedSleep()
		{
			using (var context = new TestContext(GetConnection()))
			{
				for (int i = 0, l = 10; i < l; i++)
				{
					context.GeneralProfiling.Add(new GeneralProfileModel
					{
						Number = i,
						Name = "ProfilingRead"
					});
				}

				context.SaveChanges();

				var profiler = SEProfiling.MiniProfiler.StartNew();

				var queryable = context.GeneralProfiling;
				foreach (var entity in queryable)
				{
					Thread.Sleep(100);
				}

				Assert.IsTrue(profiler.Root.CustomTimings.ContainsKey("mongodb"));
				var timings = profiler.Root.CustomTimings["mongodb"];
				Assert.IsTrue(timings[0].DurationMilliseconds > 1000);
			}
		}

		[TestMethod]
		public void ProfilingIndex()
		{
			using (var context = new TestContext(GetConnection()))
			{
				context.IndexProfiling.Add(new ProfileIndexModel());

				var profiler = SEProfiling.MiniProfiler.StartNew();

				context.SaveChanges();

				Assert.IsTrue(profiler.Root.CustomTimings.ContainsKey("mongodb"));
				var timings = profiler.Root.CustomTimings["mongodb"];
				Assert.IsTrue(timings[0].CommandString.Contains("TestIndex"));
				Assert.IsTrue(timings[0].CommandString.Contains("IndexSpecificDescriptionField"));
			}
		}
	}
}
