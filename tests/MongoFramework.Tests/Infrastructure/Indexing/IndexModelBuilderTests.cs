using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver.GeoJsonObjectModel;
using MongoFramework.Attributes;
using MongoFramework.Infrastructure.Indexing;

namespace MongoFramework.Tests.Infrastructure.Indexing.Processors
{
	[TestClass]
	public class IndexModelBuilderTests : TestBase
	{
		public class IndexNamingModel
		{
			[Index(IndexSortOrder.Ascending)]
			public string NoNameIndex { get; set; }
			[Index("MyCustomIndexName", IndexSortOrder.Ascending)]
			public string NamedIndex { get; set; }
		}

		public class IndexSortOrderModel
		{
			[Index(IndexSortOrder.Ascending)]
			public string AscendingIndex { get; set; }
			[Index(IndexSortOrder.Descending)]
			public string DescendingIndex { get; set; }
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

		public class MultikeyIndexModel
		{
			public IEnumerable<MultikeyIndexChildModel> ChildEnumerable { get; set; }
			public MultikeyIndexChildModel[] ChildArray { get; set; }
			public List<MultikeyIndexChildModel> ChildList { get; set; }
		}
		public class MultikeyIndexChildModel
		{
			[Index(IndexSortOrder.Ascending)]
			public string ChildId { get; set; }
		}

		public class TextIndexModel
		{
			[Index("MyTextIndex", IndexType.Text)]
			public string SomeTextField { get; set; }
			[Index("MyTextIndex", IndexType.Text)]
			public string AnotherTextField { get; set; }
		}
		public class Geo2dSphereIndexModel
		{
			[Index(IndexType.Geo2dSphere)]
			public GeoJsonPoint<GeoJson2DGeographicCoordinates> SomeCoordinates { get; set; }
		}
		public class TenantUniqueConstraintModel : IHaveTenantId
		{
			public string TenantId { get; set; }

			[Index("UniqueIndex", IndexSortOrder.Ascending, IsUnique = true, IsTenantExclusive = true)]
			public string UniqueIndex { get; set; }
		}

		public class MultipleIndexesOnSinglePropertyModel
		{
			[Index("MyMainIndex", IndexSortOrder.Ascending)]
			[Index("MyCompoundIndex", IndexSortOrder.Ascending, IndexPriority = 1)]
			public string MyCustomField { get; set; }

			[Index("MyCompoundIndex", IndexSortOrder.Descending, IndexPriority = 2)]
			public string OtherField { get; set; }
		}

		[TestMethod]
		public void IndexNaming()
		{
			var indexModel = IndexModelBuilder<IndexNamingModel>.BuildModel();

			Assert.AreEqual(2, indexModel.Count());
			Assert.IsTrue(indexModel.Any(m => m.Options.Name == null));
			Assert.IsTrue(indexModel.Any(m => m.Options.Name == "MyCustomIndexName"));
		}

		[TestMethod]
		public void AppliesIndexSortOrder()
		{
			var indexModel = IndexModelBuilder<IndexSortOrderModel>.BuildModel();

			Assert.AreEqual(2, indexModel.Count());

			var indexBsonDocument = indexModel.Select(m => m.Keys.Render(null, null)).ToArray();
			Assert.AreEqual(1, indexBsonDocument[0]["AscendingIndex"]);
			Assert.AreEqual(-1, indexBsonDocument[1]["DescendingIndex"]);
		}

		[TestMethod]
		public void AppliesUniqueConstraint()
		{
			var indexModel = IndexModelBuilder<UniqueConstraintModel>.BuildModel();

			Assert.AreEqual(2, indexModel.Count());
			Assert.IsTrue(indexModel.Any(m => m.Options.Name == "UniqueIndex" && m.Options.Unique == true));
			Assert.IsTrue(indexModel.Any(m => m.Options.Name == "NonUniqueIndex" && m.Options.Unique == false));
		}

		[TestMethod]
		public void CompoundIndex()
		{
			var indexModel = IndexModelBuilder<CompoundIndexModel>.BuildModel();

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
			var indexModel = IndexModelBuilder<NestedIndexBaseModel>.BuildModel();

			Assert.AreEqual(1, indexModel.Count());

			var compoundIndex = indexModel.FirstOrDefault();
			Assert.AreEqual("MyCompoundIndex", compoundIndex.Options.Name);

			var indexBsonDocument = compoundIndex.Keys.Render(null, null);

			Assert.AreEqual("ChildModel.FirstPriority", indexBsonDocument.ElementAt(0).Name);
			Assert.AreEqual("SecondPriority", indexBsonDocument.ElementAt(1).Name);
		}

		[TestMethod]
		public void MultikeyIndex()
		{
			var indexModel = IndexModelBuilder<MultikeyIndexModel>.BuildModel();

			Assert.AreEqual(3, indexModel.Count());

			var results = indexModel.Select(i => i.Keys.Render(null, null).ElementAt(0));
			Assert.IsTrue(results.Any(e => e.Name == "ChildEnumerable.ChildId"));
			Assert.IsTrue(results.Any(e => e.Name == "ChildArray.ChildId"));
			Assert.IsTrue(results.Any(e => e.Name == "ChildList.ChildId"));
		}

		[TestMethod]
		public void TextIndex()
		{
			var indexModel = IndexModelBuilder<TextIndexModel>.BuildModel();

			Assert.AreEqual(1, indexModel.Count());

			var results = indexModel.Select(i => i.Keys.Render(null, null)).FirstOrDefault();
			Assert.IsTrue(results.Any(e => e.Name == "SomeTextField" && e.Value == "text"));
			Assert.IsTrue(results.Any(e => e.Name == "AnotherTextField" && e.Value == "text"));
		}

		[TestMethod]
		public void Geo2dSphereIndex()
		{
			var indexModel = IndexModelBuilder<Geo2dSphereIndexModel>.BuildModel();

			Assert.AreEqual(1, indexModel.Count());

			var results = indexModel.Select(i => i.Keys.Render(null, null)).FirstOrDefault();
			Assert.IsTrue(results.Any(e => e.Name == "SomeCoordinates" && e.Value == "2dsphere"));
		}

		[TestMethod]
		public void AppliesTenantConstraint()
		{
			var indexModel = IndexModelBuilder<TenantUniqueConstraintModel>.BuildModel();

			var indexBsonDocument = indexModel.First().Keys.Render(null, null);

			Assert.AreEqual(1, indexBsonDocument["TenantId"].AsInt32);
			Assert.AreEqual(1, indexBsonDocument["UniqueIndex"].AsInt32);
		}

		[TestMethod]
		public void MultipleIndexesOnSingleProperty()
		{
			var indexModel = IndexModelBuilder<MultipleIndexesOnSinglePropertyModel>.BuildModel();

			Assert.AreEqual(2, indexModel.Count());

			var results = indexModel.Select(i => i.Keys.Render(null, null));
			Assert.IsTrue(results.Any(e => e.Contains("MyCustomField") && e.ElementCount == 1));
			Assert.IsTrue(results.Any(e => e.Contains("MyCustomField") && e.Contains("OtherField") && e.ElementCount == 2));
		}
	}
}