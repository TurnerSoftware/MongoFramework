using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Attributes;
using MongoFramework.Infrastructure.Mutation;
using MongoFramework.Infrastructure.Mutation.Mutators;
using System;

namespace MongoFramework.Tests.Infrastructure.Mutation.Mutators
{
	[TestClass]
	public class UpdatedDateMutatorTests : TestBase
	{
		public class InvalidAttributeUseModel
		{
			public string Id { get; set; }

			[UpdatedDate]
			public string UpdatedDate { get; set; }
		}

		public class ValidAttributeUseModel
		{
			public string Id { get; set; }

			[UpdatedDate]
			public DateTime UpdatedDate { get; set; }
		}

		[TestMethod]
		public void OnInsert()
		{
			var entity = new ValidAttributeUseModel();
			var mutator = new EntityAttributeMutator<ValidAttributeUseModel>();

			mutator.MutateEntity(entity, MutatorType.Insert);

			Assert.IsTrue(DateTime.MinValue < entity.UpdatedDate);
		}

		[TestMethod]
		public void OnUpdate()
		{
			var entity = new ValidAttributeUseModel();
			var mutator = new EntityAttributeMutator<ValidAttributeUseModel>();

			mutator.MutateEntity(entity, MutatorType.Update);

			Assert.IsTrue(DateTime.MinValue < entity.UpdatedDate);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void InvalidUseProperty()
		{
			var entity = new InvalidAttributeUseModel();
			var mutator = new EntityAttributeMutator<InvalidAttributeUseModel>();

			mutator.MutateEntity(entity, MutatorType.Insert);
		}
	}
}