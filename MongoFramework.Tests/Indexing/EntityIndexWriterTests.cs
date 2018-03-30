using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Indexing;
using System;
using System.Threading.Tasks;

namespace MongoFramework.Tests.Indexing
{
	[TestClass]
	public partial class EntityIndexWriterTests
	{
		[TestMethod]
		public void WriteIndexSync()
		{
			var database = TestConfiguration.GetDatabase();
			var collection = database.GetCollection<Models.EntityIndexWriterTests.IndexModel>("EntityIndexWriterTests.IndexModelSync");
			var indexMapper = new EntityIndexMapper<Models.EntityIndexWriterTests.IndexModel>();
			var indexWriter = new EntityIndexWriter<Models.EntityIndexWriterTests.IndexModel>(collection, indexMapper);

			indexWriter.ApplyIndexing();

			var dbIndexes = collection.Indexes.List().ToList();
			Assert.AreEqual(3, dbIndexes.Count);
		}

		[TestMethod]
		public async Task WriteIndexAsync()
		{
			var database = TestConfiguration.GetDatabase();
			var collection = database.GetCollection<Models.EntityIndexWriterTests.IndexModel>("EntityIndexWriterTests.IndexModelAsync");
			var indexMapper = new EntityIndexMapper<Models.EntityIndexWriterTests.IndexModel>();
			var indexWriter = new EntityIndexWriter<Models.EntityIndexWriterTests.IndexModel>(collection, indexMapper);

			await indexWriter.ApplyIndexingAsync();

			var dbIndexes = await collection.Indexes.List().ToListAsync();
			Assert.AreEqual(3, dbIndexes.Count);
		}

		[TestMethod]
		public void NoIndexSync()
		{
			var database = TestConfiguration.GetDatabase();
			var collection = database.GetCollection<Models.EntityIndexWriterTests.NoIndexModel>("EntityIndexWriterTests.NoIndexModelSync");
			var indexMapper = new EntityIndexMapper<Models.EntityIndexWriterTests.NoIndexModel>();
			var indexWriter = new EntityIndexWriter<Models.EntityIndexWriterTests.NoIndexModel>(collection, indexMapper);

			AssertExtensions.DoesNotThrow<Exception>(() => indexWriter.ApplyIndexing());
		}

		[TestMethod]
		public async Task NoIndexAsync()
		{
			var database = TestConfiguration.GetDatabase();
			var collection = database.GetCollection<Models.EntityIndexWriterTests.NoIndexModel>("EntityIndexWriterTests.NoIndexModelAsync");
			var indexMapper = new EntityIndexMapper<Models.EntityIndexWriterTests.NoIndexModel>();
			var indexWriter = new EntityIndexWriter<Models.EntityIndexWriterTests.NoIndexModel>(collection, indexMapper);

			await AssertExtensions.DoesNotThrowAsync<Exception>(async () => await indexWriter.ApplyIndexingAsync());
		}
	}
}