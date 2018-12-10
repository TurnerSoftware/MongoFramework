using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Attributes;
using MongoFramework.Infrastructure.Mapping;
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
			var connection = TestConfiguration.GetConnection();
			var mutator = new EntityAttributeMutator<ValidAttributeUseModel>();

			mutator.MutateEntity(entity, MutatorType.Insert, connection);

			Assert.IsTrue(DateTime.MinValue < entity.UpdatedDate);
		}

		[TestMethod]
		public void OnUpdate()
		{
			var entity = new ValidAttributeUseModel();
			var connection = TestConfiguration.GetConnection();
			var mutator = new EntityAttributeMutator<ValidAttributeUseModel>();

			mutator.MutateEntity(entity, MutatorType.Update, connection);

			Assert.IsTrue(DateTime.MinValue < entity.UpdatedDate);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void InvalidUseProperty()
		{
			var entity = new InvalidAttributeUseModel();
			var connection = TestConfiguration.GetConnection();
			var mutator = new EntityAttributeMutator<InvalidAttributeUseModel>();

			mutator.MutateEntity(entity, MutatorType.Insert, connection);
		}
	}
}