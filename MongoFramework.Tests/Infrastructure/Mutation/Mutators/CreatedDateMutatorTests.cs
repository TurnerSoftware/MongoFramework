using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Attributes;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mutation;
using MongoFramework.Infrastructure.Mutation.Mutators;
using System;

namespace MongoFramework.Tests.Infrastructure.Mutation.Mutators
{
	[TestClass]
	public class CreatedDateMutatorTests : TestBase
	{
		public class InvalidAttributeUseModel
		{
			public string Id { get; set; }

			[CreatedDate]
			public string CreatedDate { get; set; }
		}

		public class ValidAttributeUseModel
		{
			public string Id { get; set; }

			[CreatedDate]
			public DateTime CreatedDate { get; set; }
		}

		[TestMethod]
		public void OnInsertValidProperty()
		{
			var entity = new ValidAttributeUseModel();
			var entityMapper = new EntityMapper<ValidAttributeUseModel>();
			var mutator = new EntityAttributeMutator<ValidAttributeUseModel>();

			mutator.MutateEntity(entity, MutatorType.Insert, entityMapper, null);

			Assert.IsTrue(DateTime.MinValue < entity.CreatedDate);
		}

		[TestMethod]
		public void OnUpdateValidProperty()
		{
			var entity = new ValidAttributeUseModel();
			var entityMapper = new EntityMapper<ValidAttributeUseModel>();
			var mutator = new EntityAttributeMutator<ValidAttributeUseModel>();

			mutator.MutateEntity(entity, MutatorType.Update, entityMapper, null);

			Assert.AreEqual(DateTime.MinValue, entity.CreatedDate);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void InvalidUseProperty()
		{
			var entity = new InvalidAttributeUseModel();
			var entityMapper = new EntityMapper<InvalidAttributeUseModel>();
			var mutator = new EntityAttributeMutator<InvalidAttributeUseModel>();

			mutator.MutateEntity(entity, MutatorType.Insert, entityMapper, null);
		}
	}
}