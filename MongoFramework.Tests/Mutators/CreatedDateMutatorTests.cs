using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Mutation;
using MongoFramework.Infrastructure.Mutation.Mutators;
using MongoFramework.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Tests.Mutators
{
	[TestClass]
	public class CreatedDateMutatorTests
	{
		[TestMethod]
		public void OnInsert()
		{
			var entity = new AttributeEntity();
			var entityMapper = new DbEntityMapper<AttributeEntity>();
			var mutator = new EntityAttributeMutator<AttributeEntity>();

			mutator.MutateEntity(entity, MutatorType.Insert, entityMapper);

			Assert.IsTrue(DateTime.MinValue < entity.CreatedDate);
		}

		[TestMethod]
		public void OnUpdate()
		{
			var entity = new AttributeEntity();
			var entityMapper = new DbEntityMapper<AttributeEntity>();
			var mutator = new EntityAttributeMutator<AttributeEntity>();

			mutator.MutateEntity(entity, MutatorType.Update, entityMapper);

			Assert.AreEqual(DateTime.MinValue, entity.CreatedDate);
		}
	}
}
