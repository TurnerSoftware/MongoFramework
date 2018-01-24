using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Attributes;
using MongoFramework.Infrastructure.Indexing;
using MongoFramework.Infrastructure.Indexing.Processors;

namespace MongoFramework.Tests.Indexing.Processors.BasicIndex
{
	public class IndexNamingModel
	{
		[Index(IndexSortOrder.Ascending)] public string NoNameIndex { get; set; }

		[Index("MyCustomIndexName", IndexSortOrder.Ascending)]
		public string NamedIndex { get; set; }
	}

	public class IndexSortOrderModel
	{
		[Index(IndexSortOrder.Ascending)] public string AscendingIndex { get; set; }
		[Index(IndexSortOrder.Descending)] public string DescendingIndex { get; set; }
	}

	public class UniqueConstraintModel
	{
		[Index("UniqueIndex", IndexSortOrder.Ascending, IsUnique = true)]
		public string UniqueIndex { get; set; }

		[Index("NonUniqueIndex", IndexSortOrder.Ascending, IsUnique = false)]
		public string NotUniqueIndex { get; set; }
	}

	public class CompoundIndexModel
	{
		[Index("MyCompoundIndex", IndexSortOrder.Ascending, IndexPriority = 1)]
		public string FirstPriority { get; set; }

		[Index("MyCompoundIndex", IndexSortOrder.Ascending, IndexPriority = 3)]
		public string ThirdPriority { get; set; }

		[Index("MyCompoundIndex", IndexSortOrder.Ascending, IndexPriority = 2)]
		public string SecondPriority { get; set; }
	}

	public class NestedIndexBaseModel
	{
		[Index("MyCompoundIndex", IndexSortOrder.Ascending, IndexPriority = 2)]
		public string SecondPriority { get; set; }

		public NestedIndexChildModel ChildModel { get; set; }
	}

	public class NestedIndexChildModel
	{
		[Index("MyCompoundIndex", IndexSortOrder.Ascending, IndexPriority = 1)]
		public string FirstPriority { get; set; }
	}

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