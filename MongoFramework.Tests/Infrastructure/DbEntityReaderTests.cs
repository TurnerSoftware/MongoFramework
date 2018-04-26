using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using System.Linq;

namespace MongoFramework.Tests.Infrastructure
{
	[TestClass]
	public class DbEntityReaderTests : TestBase
	{
		public class A
		{
			public string Id { get; set; }
			public string Description { get; set; }
		}
		public class B : A
		{
			public bool BIsForBoolean { get; set; }
		}

		[TestMethod]
		public void ReadMixedEntities()
		{
			var database = TestConfiguration.GetDatabase();
			var entityContainer = new DbEntityCollection<A>();
			var reader = new DbEntityReader<A>(database);
			var writer = new DbEntityWriter<A>(database);

			entityContainer.Update(new A
			{
				Description = "DbEntityReaderTests.ReadMixedEntities"
			}, DbEntityEntryState.Added);

			entityContainer.Update(new B
			{
				BIsForBoolean = true,
				Description = "DbEntityReaderTests.ReadMixedEntities"
			}, DbEntityEntryState.Added);


			writer.Write(entityContainer);

			var readMixedEntitiesQuery =
				reader.AsQueryable().Where(e => e.Description == "DbEntityReaderTests.ReadMixedEntities");

			Assert.AreEqual(2, readMixedEntitiesQuery.Count());
			Assert.AreEqual(2, readMixedEntitiesQuery.OfType<A>().Count());
			Assert.AreEqual(1, readMixedEntitiesQuery.OfType<B>().Count());
		}
	}
}