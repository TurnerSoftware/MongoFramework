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
			var entityContainer = new EntityCollection<A>(connection.GetEntityMapper(typeof(A)));
			var reader = new EntityReader<A>(connection);
			var writer = new EntityWriter<A>(connection);

			entityContainer.Update(new A
			{
				Description = "DbEntityReaderTests.ReadMixedEntities"
			}, EntityEntryState.Added);

			entityContainer.Update(new B
			{
				BIsForBoolean = true,
				Description = "DbEntityReaderTests.ReadMixedEntities"
			}, EntityEntryState.Added);


			writer.Write(entityContainer);

			var readMixedEntitiesQuery =
				reader.AsQueryable().Where(e => e.Description == "DbEntityReaderTests.ReadMixedEntities");

			Assert.AreEqual(2, readMixedEntitiesQuery.Count());
			Assert.AreEqual(2, readMixedEntitiesQuery.OfType<A>().Count());
			Assert.AreEqual(1, readMixedEntitiesQuery.OfType<B>().Count());
		}
	}
}