using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Tests.Infrastructure.Mapping;

namespace MongoFramework.Tests;

[TestClass]
public class MappingBuilderTests : MappingTestBase
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
	public void ToCollection_Success()
	{
		SetupMapping(mappingBuilder =>
		{
			mappingBuilder.Entity<TestModel>()
				.ToCollection("TestCollection");
		});

		var entityDefinition = GetDefinition<TestModel>();
		Assert.AreEqual("TestCollection", entityDefinition.CollectionName);
	}

	[TestMethod]
	public void HasKey_Success()
	{
		SetupMapping(mappingBuilder =>
		{
			mappingBuilder.Entity<TestModelBase>()
				.HasKey(m => m.Id, b => b.HasKeyGenerator(EntityKeyGenerators.StringKeyGenerator));
		});

		var entityDefinition = GetDefinition<TestModelBase>();
		Assert.AreEqual(typeof(TestModelBase).GetProperty("Id"), entityDefinition.Key.Property.PropertyInfo);
	}

	[TestMethod]
	public void HasKey_WithProperty_ElementName()
	{
		SetupMapping(mappingBuilder =>
		{
			mappingBuilder.Entity<TestModelBase>()
				.HasKey(
					m => m.Id,
					b => b.HasKeyGenerator(EntityKeyGenerators.StringKeyGenerator).WithProperty(p => p.HasElementName("_id"))
				);
		});

		var entityDefinition = GetDefinition<TestModelBase>();
		Assert.AreEqual(typeof(TestModelBase).GetProperty("Id"), entityDefinition.Key.Property.PropertyInfo);
		Assert.AreEqual("_id", entityDefinition.GetIdProperty().ElementName);
	}

	[TestMethod]
	public void HasKey_KeyMustBeDefinedOnDeclaredType()
	{
		Assert.ThrowsException<ArgumentException>(() =>
			SetupMapping(mappingBuilder =>
			{
				mappingBuilder.Entity<TestModel>()
					.HasKey(
						m => m.Id,
						b => b.HasKeyGenerator(EntityKeyGenerators.StringKeyGenerator)
					);
			})
		);
	}

	[TestMethod]
	public void HasProperty_Success()
	{
		SetupMapping(mappingBuilder =>
		{
			mappingBuilder.Entity<TestModel>()
				.HasProperty(m => m.OtherName);
		});

		var entityDefinition = GetDefinition<TestModel>();
		Assert.AreEqual(typeof(TestModel).GetProperty("OtherName"), entityDefinition.Properties[0].PropertyInfo);
	}

	[TestMethod]
	public void HasProperty_HasElementName_Success()
	{
		SetupMapping(mappingBuilder =>
		{
			mappingBuilder.Entity<TestModel>()
				.HasProperty(m => m.OtherName, b => b.HasElementName("Name2"));
		});

		var entityDefinition = GetDefinition<TestModel>();
		Assert.AreEqual(typeof(TestModel).GetProperty("OtherName"), entityDefinition.Properties[0].PropertyInfo);
		Assert.AreEqual("Name2", entityDefinition.Properties[0].ElementName);
	}

	[TestMethod]
	public void HasProperty_PropertyMustBeDefinedOnDeclaredType()
	{
		Assert.ThrowsException<ArgumentException>(() =>
			SetupMapping(mappingBuilder =>
			{
				mappingBuilder.Entity<TestModel>()
					.HasProperty(m => m.Name);
			})
		);
	}

	[TestMethod]
	public void HasIndex_SingleProperty_Success()
	{
		SetupMapping(mappingBuilder =>
		{
			mappingBuilder.Entity<TestModel>()
				.HasIndex(m => m.SomethingIndexable, b =>
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
				.HasIndex(m => new
				{
					m.SomethingIndexable,
					m.OtherName
				}, b =>
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
				.HasIndex(m => new
				{
					m.SomethingIndexable,
					m.OtherName,
					m.OneOfThem.Description
				}, b =>
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
				.HasIndex(m => new
				{
					m.SomethingIndexable,
					m.OtherName,
					m.OneOfThem.Description,
					m.ManyOfThem.First().AnotherThingIndexable
				}, b =>
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
	public void HasIndex_IndexPropertiesAreMappedAsProperties_Success()
	{
		SetupMapping(mappingBuilder =>
		{
			mappingBuilder.Entity<TestModel>()
				.HasIndex(m => new
				{
					m.SomethingIndexable,
					m.OneOfThem.Description,
					m.ManyOfThem.First().AnotherThingIndexable
				});
		});

		var testModelDefinition = GetDefinition<TestModel>();
		Assert.AreEqual(typeof(TestModel).GetProperty("SomethingIndexable"), testModelDefinition.GetProperty("SomethingIndexable").PropertyInfo);
		Assert.AreEqual(typeof(TestModel).GetProperty("OneOfThem"), testModelDefinition.GetProperty("OneOfThem").PropertyInfo);
		Assert.AreEqual(typeof(TestModelBase).GetProperty("ManyOfThem"), testModelDefinition.GetProperty("ManyOfThem").PropertyInfo);

		var nestedModelDefinition = GetDefinition<NestedModel>();
		Assert.AreEqual(typeof(NestedModelBase).GetProperty("Description"), nestedModelDefinition.GetProperty("Description").PropertyInfo);
		Assert.AreEqual(typeof(NestedModel).GetProperty("AnotherThingIndexable"), nestedModelDefinition.GetProperty("AnotherThingIndexable").PropertyInfo);
	}

	[TestMethod]
	public void HasIndex_ElementNamesApplyToIndexPath_Success()
	{
		SetupMapping(mappingBuilder =>
		{
			mappingBuilder.Entity<TestModel>()
				.HasIndex(m => new
				{
					m.SomethingIndexable,
					m.OneOfThem.Description,
					m.ManyOfThem.First().AnotherThingIndexable
				})
				.HasProperty(m => m.SomethingIndexable, b => b.HasElementName("QuiteIndexable"));

			mappingBuilder.Entity<NestedModelBase>()
				.HasProperty(m => m.Description, b => b.HasElementName("VeryDescriptive"));
			mappingBuilder.Entity<TestModelBase>()
				.HasProperty(m => m.ManyOfThem, b => b.HasElementName("SoManyOfThem"));
		});

		var entityDefinition = GetDefinition<TestModel>();
		var indexPathDefinitions = entityDefinition.Indexes[0].IndexPaths;
		Assert.AreEqual("QuiteIndexable", indexPathDefinitions[0].Path);
		Assert.AreEqual("OneOfThem.VeryDescriptive", indexPathDefinitions[1].Path);
		Assert.AreEqual("SoManyOfThem.AnotherThingIndexable", indexPathDefinitions[2].Path);
	}

	[TestMethod]
	public void HasExtraElements_Success()
	{
		SetupMapping(mappingBuilder =>
		{
			mappingBuilder.Entity<TestModel>()
				.HasExtraElements(m => m.ExtraElements);
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
				.Ignore(m => m.Id)
				.WithDerivedEntity<TestModel>(b =>
				{
					b.Ignore(m => m.OtherName)
						.Ignore(m => m.ExtraElements)
						.Ignore(m => m.SomethingIndexable);
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
