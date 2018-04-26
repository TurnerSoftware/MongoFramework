using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using MongoFramework.Attributes;
using MongoFramework.Infrastructure.Indexing;
using System;
using System.Threading.Tasks;

namespace MongoFramework.Tests.Infrastructure.Indexing
{
	[TestClass]
	public class EntityIndexWriterTests : TestBase
	{
		public class IndexModel
		{
			public string Id { get; set; }
			[Index(IndexSortOrder.Ascending)]
			public string IndexedPropertyOne { get; set; }
			[Index("MyIndexedProperty", IndexSortOrder.Descending)]
			public string IndexedPropertyTwo { get; set; }
		}

		public class NoIndexModel
		{
			public string Id { get; set; }
		}

		[TestMethod]
		public void WriteIndexSync()
		{
			var database = TestConfiguration.GetDatabase();
			var collection = database.GetCollection<IndexModel>("EntityIndexWriterTests.IndexModelSync");
			var indexMapper = new EntityIndexMapper<IndexModel>();
			var indexWriter = new EntityIndexWriter<IndexModel>(collection, indexMapper);

			indexWriter.ApplyIndexing();

			var dbIndexes = collection.Indexes.List().ToList();
			Assert.AreEqual(3, dbIndexes.Count);
		}

		[TestMethod]
		public async Task WriteIndexAsync()
		{
			var database = TestConfiguration.GetDatabase();
			var collection = database.GetCollection<IndexModel>("EntityIndexWriterTests.IndexModelAsync");
			var indexMapper = new EntityIndexMapper<IndexModel>();
			var indexWriter = new EntityIndexWriter<IndexModel>(collection, indexMapper);

			await indexWriter.ApplyIndexingAsync().ConfigureAwait(false);

			var dbIndexes = await collection.Indexes.List().ToListAsync().ConfigureAwait(false);
			Assert.AreEqual(3, dbIndexes.Count);
		}

		[TestMethod]
		public void NoIndexSync()
		{
			var database = TestConfiguration.GetDatabase();
			var collection = database.GetCollection<NoIndexModel>("EntityIndexWriterTests.NoIndexModelSync");
			var indexMapper = new EntityIndexMapper<NoIndexModel>();
			var indexWriter = new EntityIndexWriter<NoIndexModel>(collection, indexMapper);

			AssertExtensions.DoesNotThrow<Exception>(() => indexWriter.ApplyIndexing());
		}

		[TestMethod]
		public async Task NoIndexAsync()
		{
			var database = TestConfiguration.GetDatabase();
			var collection = database.GetCollection<NoIndexModel>("EntityIndexWriterTests.NoIndexModelAsync");
			var indexMapper = new EntityIndexMapper<NoIndexModel>();
			var indexWriter = new EntityIndexWriter<NoIndexModel>(collection, indexMapper);

			await AssertExtensions.DoesNotThrowAsync<Exception>(async () => await indexWriter.ApplyIndexingAsync().ConfigureAwait(false)).ConfigureAwait(false);
		}
	}
}