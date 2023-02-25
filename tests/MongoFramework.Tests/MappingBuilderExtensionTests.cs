using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Tests.Infrastructure.Mapping;

namespace MongoFramework.Tests;

[TestClass]
public class MappingBuilderExtensionTests : MappingTestBase
{
	public class TestModelBase
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public IEnumerable<NestedModel> ManyOfThem { get; set; }
	}

	public class TestModel : TestModelBase
	{
		public Dictionary<string, object> ExtraElements { get; set; }
		public string OtherName { get; set; }
		public int SomethingIndexable { get; set; }
		public NestedModelBase OneOfThem { get; set; }
	}

	public class NestedModelBase
	{
		public string Description { get; set; }
	}

	public class NestedModel : NestedModelBase
	{
		public string Address { get; set; }
		public int AnotherThingIndexable { get; set; }
	}

	private static void SetupMapping(Action<MappingBuilder> builder)
	{
		var mappingBuilder = new MappingBuilder(Array.Empty<IMappingProcessor>());
		builder(mappingBuilder);
		EntityMapping.RegisterMapping(mappingBuilder);
	}
	private static EntityDefinition GetDefinition<TEntity>()
	{
		return EntityMapping.GetOrCreateDefinition(typeof(TEntity));
	}

	[TestMethod]
	public void HasKey_Success()
	{
		SetupMapping(mappingBuilder =>
		{
			mappingBuilder.Entity<TestModelBase>()
				.HasKey("Id", b => b.HasKeyGenerator(EntityKeyGenerators.StringKeyGenerator));
		});

		var entityDefinition = GetDefinition<TestModelBase>();
		Assert.AreEqual(typeof(TestModelBase).GetProperty("Id"), entityDefinition.Key.Property.PropertyInfo);
	}

	[TestMethod]
	public void HasProperty_Success()
	{
		SetupMapping(mappingBuilder =>
		{
			mappingBuilder.Entity<TestModel>()
				.HasProperty("OtherName");
		});

		var entityDefinition = GetDefinition<TestModel>();
		Assert.AreEqual(typeof(TestModel).GetProperty("OtherName"), entityDefinition.Properties[0].PropertyInfo);
	}

	[TestMethod]
	public void HasIndex_SingleProperty_Success()
	{
		SetupMapping(mappingBuilder =>
		{
			mappingBuilder.Entity<TestModel>()
				.HasIndex(new[] { "SomethingIndexable" }, b =>
				{
					b.HasName("MyIndexable")
						.IsDescending(true)
						.IsUnique();
				});
		});

		var entityDefinition = GetDefinition<TestModel>();
		var indexPathDefinition = entityDefinition.Indexes[0].IndexPaths[0];
		Assert.AreEqual("MyIndexable", entityDefinition.Indexes[0].IndexName);
		Assert.IsTrue(entityDefinition.Indexes[0].IsUnique);
		Assert.AreEqual("SomethingIndexable", indexPathDefinition.Path);
		Assert.AreEqual(IndexType.Standard, indexPathDefinition.IndexType);
		Assert.AreEqual(IndexSortOrder.Descending, indexPathDefinition.SortOrder);
	}

	[TestMethod]
	public void HasIndex_MultiProperty_Success()
	{
		SetupMapping(mappingBuilder =>
		{
			mappingBuilder.Entity<TestModel>()
				.HasIndex(new[] { "SomethingIndexable", "OtherName" }, b =>
				{
					b.HasName("MyIndexable")
						.IsDescending(true)
						.HasType(IndexType.Standard, IndexType.Text)
						.IsUnique();
				});
		});

		var entityDefinition = GetDefinition<TestModel>();
		var indexPathDefinitions = entityDefinition.Indexes[0].IndexPaths;
		Assert.AreEqual("MyIndexable", entityDefinition.Indexes[0].IndexName);
		Assert.IsTrue(entityDefinition.Indexes[0].IsUnique);
		Assert.AreEqual("SomethingIndexable", indexPathDefinitions[0].Path);
		Assert.AreEqual(IndexType.Standard, indexPathDefinitions[0].IndexType);
		Assert.AreEqual(IndexSortOrder.Descending, indexPathDefinitions[0].SortOrder);
		Assert.AreEqual("OtherName", indexPathDefinitions[1].Path);
		Assert.AreEqual(IndexType.Text, indexPathDefinitions[1].IndexType);
		Assert.AreEqual(IndexSortOrder.Ascending, indexPathDefinitions[1].SortOrder);
	}

	[TestMethod]
	public void HasIndex_NestedProperties_Success()
	{
		SetupMapping(mappingBuilder =>
		{
			mappingBuilder.Entity<TestModel>()
				.HasIndex(new[] { "SomethingIndexable", "OtherName", "OneOfThem.Description" }, b =>
				{
					b.HasName("MyIndexable")
						.IsDescending(true)
						.HasType(IndexType.Standard, IndexType.Text, IndexType.Text)
						.IsUnique();
				});
		});

		var entityDefinition = GetDefinition<TestModel>();
		var indexPathDefinitions = entityDefinition.Indexes[0].IndexPaths;
		Assert.AreEqual("MyIndexable", entityDefinition.Indexes[0].IndexName);
		Assert.IsTrue(entityDefinition.Indexes[0].IsUnique);
		Assert.AreEqual("SomethingIndexable", indexPathDefinitions[0].Path);
		Assert.AreEqual(IndexType.Standard, indexPathDefinitions[0].IndexType);
		Assert.AreEqual(IndexSortOrder.Descending, indexPathDefinitions[0].SortOrder);
		Assert.AreEqual("OtherName", indexPathDefinitions[1].Path);
		Assert.AreEqual(IndexType.Text, indexPathDefinitions[1].IndexType);
		Assert.AreEqual(IndexSortOrder.Ascending, indexPathDefinitions[1].SortOrder);
		Assert.AreEqual("OneOfThem.Description", indexPathDefinitions[2].Path);
		Assert.AreEqual(IndexType.Text, indexPathDefinitions[2].IndexType);
		Assert.AreEqual(IndexSortOrder.Ascending, indexPathDefinitions[2].SortOrder);
	}

	[TestMethod]
	public void HasIndex_NestedPropertyThroughEnumerable_Success()
	{
		SetupMapping(mappingBuilder =>
		{
			mappingBuilder.Entity<TestModel>()
				.HasIndex(new[] { "SomethingIndexable", "OtherName", "OneOfThem.Description", "ManyOfThem.AnotherThingIndexable" }, b =>
				{
					b.HasName("MyIndexable")
						.IsDescending(true, false, false, true)
						.HasType(IndexType.Standard, IndexType.Text, IndexType.Text)
						.IsUnique();
				});
		});

		var entityDefinition = GetDefinition<TestModel>();
		var indexPathDefinitions = entityDefinition.Indexes[0].IndexPaths;
		Assert.AreEqual("MyIndexable", entityDefinition.Indexes[0].IndexName);
		Assert.IsTrue(entityDefinition.Indexes[0].IsUnique);
		Assert.AreEqual("SomethingIndexable", indexPathDefinitions[0].Path);
		Assert.AreEqual(IndexType.Standard, indexPathDefinitions[0].IndexType);
		Assert.AreEqual(IndexSortOrder.Descending, indexPathDefinitions[0].SortOrder);
		Assert.AreEqual("OtherName", indexPathDefinitions[1].Path);
		Assert.AreEqual(IndexType.Text, indexPathDefinitions[1].IndexType);
		Assert.AreEqual(IndexSortOrder.Ascending, indexPathDefinitions[1].SortOrder);
		Assert.AreEqual("OneOfThem.Description", indexPathDefinitions[2].Path);
		Assert.AreEqual(IndexType.Text, indexPathDefinitions[2].IndexType);
		Assert.AreEqual(IndexSortOrder.Ascending, indexPathDefinitions[2].SortOrder);
		Assert.AreEqual("ManyOfThem.AnotherThingIndexable", indexPathDefinitions[3].Path);
		Assert.AreEqual(IndexType.Standard, indexPathDefinitions[3].IndexType);
		Assert.AreEqual(IndexSortOrder.Descending, indexPathDefinitions[3].SortOrder);
	}

	[TestMethod]
	public void HasExtraElements_Success()
	{
		SetupMapping(mappingBuilder =>
		{
			mappingBuilder.Entity<TestModel>()
				.HasExtraElements("ExtraElements");
		});

		var entityDefinition = GetDefinition<TestModel>();
		Assert.AreEqual(typeof(TestModel).GetProperty("ExtraElements"), entityDefinition.ExtraElements.Property.PropertyInfo);
		Assert.IsFalse(entityDefinition.ExtraElements.IgnoreExtraElements);
	}

	[TestMethod]
	public void Ignore_RemovesAllPropertyReferences_Success()
	{
		SetupMapping(mappingBuilder =>
		{
			mappingBuilder.Entity<TestModelBase>()
				.HasKey(m => m.Id, b => b.HasKeyGenerator(EntityKeyGenerators.StringKeyGenerator))
				.WithDerivedEntity<TestModel>(b =>
				{
					b.HasProperty(m => m.OtherName)
						.HasExtraElements(m => m.ExtraElements)
						.HasIndex(m => m.SomethingIndexable)
						.HasIndex(m => new
						{
							m.SomethingIndexable,
							m.OneOfThem.Description
						});
				});

			mappingBuilder.Entity<TestModelBase>()
				.Ignore("Id")
				.WithDerivedEntity(typeof(TestModel), b =>
				{
					b.Ignore("OtherName")
						.Ignore("ExtraElements")
						.Ignore("SomethingIndexable");
				});
		});

		var testModelDefinition = GetDefinition<TestModel>();
		Assert.AreEqual(1, testModelDefinition.Properties.Count);
		Assert.AreEqual(typeof(TestModel).GetProperty("OneOfThem"), testModelDefinition.Properties[0].PropertyInfo);
		Assert.IsTrue(testModelDefinition.ExtraElements.IgnoreExtraElements);
		Assert.AreEqual(0, testModelDefinition.Indexes.Count);

		var testModelBaseDefinition = GetDefinition<TestModelBase>();
		Assert.IsNull(testModelBaseDefinition.Key);
		Assert.AreEqual(0, testModelBaseDefinition.Properties.Count);
	}
}
