using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Attributes;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mutation;
using MongoFramework.Infrastructure.Mutation.Mutators;

namespace MongoFramework.Tests.Mutation.Mutators.UpdatedDate
{
	public class ValidAttributeUseModel
	{
		public string Id { get; set; }

		[UpdatedDate]
		public DateTime UpdatedDate { get; set; }
	}

	public class InvalidAttributeUseModel
	{
		public string Id { get; set; }

		[UpdatedDate]
		public string UpdatedDate { get; set; }
	}

	[TestClass]
	public class UpdatedDateMutatorTests
	{
		[TestMethod]
		public void OnInsert()
		{
			var entity = new ValidAttributeUseModel();
			var entityMapper = new EntityMapper<ValidAttributeUseModel>();
			var mutator = new EntityAttributeMutator<ValidAttributeUseModel>();

			mutator.MutateEntity(entity, MutatorType.Insert, entityMapper);

			Assert.IsTrue(DateTime.MinValue < entity.UpdatedDate);
		}

		[TestMethod]
		public void OnUpdate()
		{
			var entity = new ValidAttributeUseModel();
			var entityMapper = new EntityMapper<ValidAttributeUseModel>();
			var mutator = new EntityAttributeMutator<ValidAttributeUseModel>();

			mutator.MutateEntity(entity, MutatorType.Update, entityMapper);

			Assert.IsTrue(DateTime.MinValue < entity.UpdatedDate);
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