using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
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
			[Index(IndexType.Text)]
			public string IndexedText { get; set; }
			[Index(IndexType.Geo2dSphere)]
			public GeoJsonPoint<GeoJson2DGeographicCoordinates> IndexedGeo { get; set; }
		}

		public class NoIndexModel
		{
			public string Id { get; set; }
		}

		[TestMethod]
		public void WriteIndexSync()
		{
			var connection = TestConfiguration.GetConnection();
			var indexWriter = new EntityIndexWriter<IndexModel>(connection);

			indexWriter.ApplyIndexing();

			var collection = connection.GetDatabase().GetCollection<IndexModel>("IndexModel");
			var dbIndexes = collection.Indexes.List().ToList();
			Assert.AreEqual(5, dbIndexes.Count);
		}

		[TestMethod]
		public async Task WriteIndexAsync()
		{
			var connection = TestConfiguration.GetConnection();
			var indexWriter = new EntityIndexWriter<IndexModel>(connection);

			await indexWriter.ApplyIndexingAsync().ConfigureAwait(false);

			var collection = connection.GetDatabase().GetCollection<IndexModel>("IndexModel");
			var dbIndexes = await collection.Indexes.List().ToListAsync().ConfigureAwait(false);
			Assert.AreEqual(5, dbIndexes.Count);
		}

		[TestMethod]
		public void NoIndexSync()
		{
			var connection = TestConfiguration.GetConnection();
			var indexWriter = new EntityIndexWriter<NoIndexModel>(connection);

			AssertExtensions.DoesNotThrow<Exception>(() => indexWriter.ApplyIndexing());
		}

		[TestMethod]
		public async Task NoIndexAsync()
		{
			var connection = TestConfiguration.GetConnection();
			var indexWriter = new EntityIndexWriter<NoIndexModel>(connection);

			await AssertExtensions.DoesNotThrowAsync<Exception>(async () => await indexWriter.ApplyIndexingAsync().ConfigureAwait(false)).ConfigureAwait(false);
		}
	}
}