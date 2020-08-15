using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using System.Linq;

namespace MongoFramework.Tests.Infrastructure
{
	[TestClass]
	public class EntityReaderTests : TestBase
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
			var connection = TestConfiguration.GetConnection();
			var entityCollection = new EntityCollection<A>();
			var reader = new EntityReader<A>(connection);
			var commandWriter = new CommandWriter<A>(connection);

			entityCollection.Update(new A
			{
				Description = "DbEntityReaderTests.ReadMixedEntities"
			}, EntityEntryState.Added);

			entityCollection.Update(new B
			{
				BIsForBoolean = true,
				Description = "DbEntityReaderTests.ReadMixedEntities"
			}, EntityEntryState.Added);

			commandWriter.Write(entityCollection.GetEntries().Select(e => EntityCommandBuilder<A>.CreateCommand(e)));

			var readMixedEntitiesQuery =
				reader.AsQueryable().Where(e => e.Description == "DbEntityReaderTests.ReadMixedEntities");

			Assert.AreEqual(2, readMixedEntitiesQuery.Count());
			Assert.AreEqual(2, readMixedEntitiesQuery.OfType<A>().Count());
			Assert.AreEqual(1, readMixedEntitiesQuery.OfType<B>().Count());
		}
	}
}