using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Attributes;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mutation;
using MongoFramework.Infrastructure.Mutation.Mutators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MongoFramework.Tests.Infrastructure.Mutation.Mutators
{
	[TestClass]
	public class PreallocateArrayMutatorTests : TestBase
	{
		public class GenericTypeHiddenModel
		{
			public string Id { get; set; }

			[PreallocateArray(5)]
			public GenericTypeHiddenList MyItems { get; set; }
		}

		public class GenericTypeHiddenList : List<TestItem> { }

		public class AltogetherWrongTypeModel
		{
			public string Id { get; set; }

			[PreallocateArray(5)]
			public object MyItems { get; set; }
		}

		public class InvalidNumberOfItemsModel
		{
			public string Id { get; set; }

			[PreallocateArray(-1)]
			public IEnumerable<TestItem> MyItems { get; set; }
		}

		public class ValidAttributeUseModel
		{
			public string Id { get; set; }

			[PreallocateArray(5)]
			public IEnumerable<TestItem> MyItems { get; set; }
		}

		public class TestItem
		{
			public string Name { get; set; }
		}

		[TestMethod]
		public void NoExistingItems()
		{
			var entity = new ValidAttributeUseModel();
			var entityMapper = new EntityMapper<ValidAttributeUseModel>();
			var mutator = new EntityAttributeMutator<ValidAttributeUseModel>();

			mutator.MutateEntity(entity, MutatorType.Insert, entityMapper, null);

			var expected = new[] { new TestItem(), new TestItem(), new TestItem(), new TestItem(), new TestItem() };

			Assert.AreEqual(5, entity.MyItems.Count());
			Assert.IsNotNull(entity.MyItems.ElementAt(0));
		}

		[TestMethod]
		public void SomeExistingItems()
		{
			var entity = new ValidAttributeUseModel
			{
				MyItems = new[] { new TestItem { Name = "A" } }
			};

			var entityMapper = new EntityMapper<ValidAttributeUseModel>();
			var mutator = new EntityAttributeMutator<ValidAttributeUseModel>();

			Assert.AreEqual(1, entity.MyItems.Count());

			mutator.MutateEntity(entity, MutatorType.Insert, entityMapper, null);

			var expected = new[] { new TestItem { Name = "A" }, new TestItem(), new TestItem(), new TestItem(), new TestItem() };

			Assert.AreEqual(5, entity.MyItems.Count());
			Assert.IsNotNull(entity.MyItems.ElementAt(0));
			Assert.IsNotNull(entity.MyItems.ElementAt(1));
			Assert.AreEqual(expected[0].Name, entity.MyItems.ElementAt(0).Name);
		}

		[TestMethod]
		public void MoreThanEnoughExistingItems()
		{
			var entity = new ValidAttributeUseModel
			{
				MyItems = new[] { new TestItem(), new TestItem(), new TestItem(), new TestItem(), new TestItem(), new TestItem() }
			};

			var entityMapper = new EntityMapper<ValidAttributeUseModel>();
			var mutator = new EntityAttributeMutator<ValidAttributeUseModel>();

			Assert.AreEqual(6, entity.MyItems.Count());

			mutator.MutateEntity(entity, MutatorType.Insert, entityMapper, null);

			var expected = new[] { new TestItem(), new TestItem(), new TestItem(), new TestItem(), new TestItem(), new TestItem() };

			Assert.AreEqual(6, entity.MyItems.Count());
			Assert.IsNotNull(entity.MyItems.ElementAt(0));
			Assert.IsNotNull(entity.MyItems.ElementAt(5));
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void InvalidNumberOfItems()
		{
			var entity = new InvalidNumberOfItemsModel();
			var entityMapper = new EntityMapper<InvalidNumberOfItemsModel>();
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void WrongPropertyType()
		{
			var entity = new AltogetherWrongTypeModel();
			var entityMapper = new EntityMapper<AltogetherWrongTypeModel>();
			var mutator = new EntityAttributeMutator<AltogetherWrongTypeModel>();

			mutator.MutateEntity(entity, MutatorType.Insert, entityMapper, null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void HiddenGenericType()
		{
			var entity = new GenericTypeHiddenModel();
			var entityMapper = new EntityMapper<GenericTypeHiddenModel>();
			var mutator = new EntityAttributeMutator<GenericTypeHiddenModel>();

			mutator.MutateEntity(entity, MutatorType.Insert, entityMapper, null);
		}
	}
}