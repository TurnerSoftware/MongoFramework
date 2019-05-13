using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

			public MongoDbSet<Person> People { get; set; }
		}

		class Person
		{
			public string Id { get; set; }
			public string Name { get; set; }
			public int Age { get; set; }
		}

		[TestMethod]
		public void ProfilingInsert()
		{
			using (var context = new TestContext(GetConnection()))
			{
				var profiler = SEProfiling.MiniProfiler.StartNew();
				context.People.Add(new Person
				{
					Name = "ProfilingInsert",
				});
				context.SaveChanges();

				Assert.IsTrue(profiler.Root.CustomTimings.ContainsKey("mongoframework"));
				var timings = profiler.Root.CustomTimings["mongoframework"];
				Assert.IsTrue(timings[0].CommandString.Contains("InsertOne"));
				Assert.IsTrue(timings[0].CommandString.Contains("ProfilingInsert"));
			}
		}

		[TestMethod]
		public void ProfilingUpdate()
		{
			using (var context = new TestContext(GetConnection()))
			{
				var entity = new Person
				{
					Name = "ProfilingUpdate",
				};
				context.People.Add(entity);
				context.SaveChanges();

				var profiler = SEProfiling.MiniProfiler.StartNew();

				entity.Name = "ProfilingUpdate-Updated";
				context.SaveChanges();

				Assert.IsTrue(profiler.Root.CustomTimings.ContainsKey("mongoframework"));
				var timings = profiler.Root.CustomTimings["mongoframework"];
				Assert.IsTrue(timings[0].CommandString.Contains("UpdateOne"));
				Assert.IsTrue(timings[0].CommandString.Contains("ProfilingUpdate-Updated"));
			}
		}

		[TestMethod]
		public void ProfilingDelete()
		{
			using (var context = new TestContext(GetConnection()))
			{
				var entity = new Person
				{
					Name = "ProfilingDelete",
				};
				context.People.Add(entity);
				context.SaveChanges();

				var profiler = SEProfiling.MiniProfiler.StartNew();

				context.People.Remove(entity);
				context.SaveChanges();

				Assert.IsTrue(profiler.Root.CustomTimings.ContainsKey("mongoframework"));
				var timings = profiler.Root.CustomTimings["mongoframework"];
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
					context.People.Add(new Person
					{
						Age = i,
						Name = "ProfilingRead"
					});
				}
				
				context.SaveChanges();

				var profiler = SEProfiling.MiniProfiler.StartNew();

				var queryable = context.People.Skip(78);
				foreach (var entity in queryable)
				{ }

				Assert.IsTrue(profiler.Root.CustomTimings.ContainsKey("mongoframework"));
				var timings = profiler.Root.CustomTimings["mongoframework"];
				Assert.IsTrue(timings[0].CommandString.Contains("$skip"));
				Assert.IsTrue(timings[0].CommandString.Contains("78"));
			}
		}
	}
}
