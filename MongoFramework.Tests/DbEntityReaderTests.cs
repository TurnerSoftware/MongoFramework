using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using MongoFramework.Tests.Models;
using System.Linq;

namespace MongoFramework.Tests
{
	[TestClass]
	public class DbEntityReaderTests
	{
		[TestMethod]
		public void ReadMixedEntities()
		{
			var database = TestConfiguration.GetDatabase();
			var entityContainer = new DbEntityCollection<CommonEntity>();
			var reader = new DbEntityReader<CommonEntity>(database);
			var writer = new DbEntityWriter<CommonEntity>(database);

			entityContainer.Update(new CommonEntity
			{
				Description = "DbEntityReaderTests.ReadMixedEntities"
			}, DbEntityEntryState.Added);

			entityContainer.Update(new ExtendedEntity
			{
				IsDisabled = true,
				Description = "DbEntityReaderTests.ReadMixedEntities"
			}, DbEntityEntryState.Added);


			writer.Write(entityContainer);

			var readMixedEntitiesQuery =
				reader.AsQueryable().Where(e => e.Description == "DbEntityReaderTests.ReadMixedEntities");

			Assert.AreEqual(2, readMixedEntitiesQuery.Count());
			Assert.AreEqual(2, readMixedEntitiesQuery.OfType<CommonEntity>().Count());
			Assert.AreEqual(1, readMixedEntitiesQuery.OfType<ExtendedEntity>().Count());
		}
	}
}