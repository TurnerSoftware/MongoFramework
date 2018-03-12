using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mutation;
using MongoFramework.Infrastructure.Mutation.Mutators;

namespace MongoFramework.Tests.Mutation.Mutators.CreatedDate
{
	[TestClass]
	public class CreatedDateMutatorTests
	{
		[TestMethod]
		public void OnInsertValidProperty()
		{
			var entity = new ValidAttributeUseModel();
			var entityMapper = new EntityMapper<ValidAttributeUseModel>();
			var mutator = new EntityAttributeMutator<ValidAttributeUseModel>();

			mutator.MutateEntity(entity, MutatorType.Insert, entityMapper);

			Assert.IsTrue(DateTime.MinValue < entity.CreatedDate);
		}

		[TestMethod]
		public void OnUpdateValidProperty()
		{
			var entity = new ValidAttributeUseModel();
			var entityMapper = new EntityMapper<ValidAttributeUseModel>();
			var mutator = new EntityAttributeMutator<ValidAttributeUseModel>();

			mutator.MutateEntity(entity, MutatorType.Update, entityMapper);

			Assert.AreEqual(DateTime.MinValue, entity.CreatedDate);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void InvalidUseProperty()
		{
			var entity = new InvalidAttributeUseModel();
			var entityMapper = new EntityMapper<InvalidAttributeUseModel>();
			var mutator = new EntityAttributeMutator<InvalidAttributeUseModel>();

			mutator.MutateEntity(entity, MutatorType.Insert, entityMapper);
		}
	}
}