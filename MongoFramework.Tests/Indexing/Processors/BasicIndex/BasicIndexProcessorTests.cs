using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure.Indexing;
using MongoFramework.Infrastructure.Indexing.Processors;
using MongoFramework.Tests.Models;
using System.Linq;

namespace MongoFramework.Tests.Indexing.Processors.BasicIndex
{
	[TestClass]
	public class BasicIndexProcessorTests
	{
		[TestMethod]
		public void IndexNaming()
		{
			var processor = new BasicIndexProcessor();
			var entityIndexMapper = new EntityIndexMapper<IndexNamingModel>();
			var indexMapping = entityIndexMapper.GetIndexMapping();

			var indexModel = processor.BuildIndexModel<IndexNamingModel>(indexMapping);

			Assert.AreEqual(2, indexModel.Count());
			Assert.IsTrue(indexModel.Any(m => m.Options.Name == null));
			Assert.IsTrue(indexModel.Any(m => m.Options.Name == "MyCustomIndexName"));
		}

		[TestMethod]
		public void IndexSortOrder()
		{
			var processor = new BasicIndexProcessor();
			var entityIndexMapper = new EntityIndexMapper<IndexSortOrderModel>();
			var indexMapping = entityIndexMapper.GetIndexMapping();

			var indexModel = processor.BuildIndexModel<IndexSortOrderModel>(indexMapping);

			Assert.AreEqual(2, indexModel.Count());

			var indexBsonDocument = indexModel.Select(m => m.Keys.Render(null, null)).ToArray();
			Assert.AreEqual(1, indexBsonDocument[0]["AscendingIndex"]);
			Assert.AreEqual(-1, indexBsonDocument[1]["DescendingIndex"]);
		}

		[TestMethod]
		public void IndexUniqueConstraint()
		{
			var processor = new BasicIndexProcessor();
			var entityIndexMapper = new EntityIndexMapper<UniqueConstraintModel>();
			var indexMapping = entityIndexMapper.GetIndexMapping();

			var indexModel = processor.BuildIndexModel<UniqueConstraintModel>(indexMapping);

			Assert.AreEqual(2, indexModel.Count());
			Assert.IsTrue(indexModel.Any(m => m.Options.Name == "UniqueIndex" && m.Options.Unique == true));
			Assert.IsTrue(indexModel.Any(m => m.Options.Name == "NonUniqueIndex" && m.Options.Unique == false));
		}

		[TestMethod]
		public void CompoundIndex()
		{
			var processor = new BasicIndexProcessor();
			var entityIndexMapper = new EntityIndexMapper<CompoundIndexModel>();
			var indexMapping = entityIndexMapper.GetIndexMapping();

			var indexModel = processor.BuildIndexModel<CompoundIndexModel>(indexMapping);

			Assert.AreEqual(1, indexModel.Count());

			var compoundIndex = indexModel.FirstOrDefault();
			Assert.AreEqual("MyCompoundIndex", compoundIndex.Options.Name);

			var indexBsonDocument = compoundIndex.Keys.Render(null, null);

			Assert.AreEqual("FirstPriority", indexBsonDocument.ElementAt(0).Name);
			Assert.AreEqual("SecondPriority", indexBsonDocument.ElementAt(1).Name);
			Assert.AreEqual("ThirdPriority", indexBsonDocument.ElementAt(2).Name);
		}

		[TestMethod]
		public void NestedCompoundIndex()
		{
			var processor = new BasicIndexProcessor();
			var entityIndexMapper = new EntityIndexMapper<NestedIndexBaseModel>();
			var indexMapping = entityIndexMapper.GetIndexMapping();

			var indexModel = processor.BuildIndexModel<NestedIndexBaseModel>(indexMapping);

			Assert.AreEqual(1, indexModel.Count());

			var compoundIndex = indexModel.FirstOrDefault();
			Assert.AreEqual("MyCompoundIndex", compoundIndex.Options.Name);

			var indexBsonDocument = compoundIndex.Keys.Render(null, null);

			Assert.AreEqual("ChildModel.FirstPriority", indexBsonDocument.ElementAt(0).Name);
			Assert.AreEqual("SecondPriority", indexBsonDocument.ElementAt(1).Name);
		}
	}
}