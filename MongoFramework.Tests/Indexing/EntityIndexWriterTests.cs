using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using MongoFramework.Attributes;
using MongoFramework.Infrastructure.Indexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Tests.Indexing
{
	[TestClass]
	public class EntityIndexWriterTests
	{
		public class IndexModel
		{
			public string Id { get; set; }
			[Index(IndexSortOrder.Ascending)]
			public string IndexedPropertyOne { get; set; }
			[Index("MyIndexedProperty", IndexSortOrder.Descending)]
			public string IndexedPropertyTwo { get; set; }
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

			await indexWriter.ApplyIndexingAsync();

			var dbIndexes = await collection.Indexes.List().ToListAsync();
			Assert.AreEqual(3, dbIndexes.Count);
		}
	}
}
