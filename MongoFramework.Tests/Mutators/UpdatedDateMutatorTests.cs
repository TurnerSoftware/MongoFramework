using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Mapping;
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
	public class UpdatedDateMutatorTests
	{
		[TestMethod]
		public void OnInsert()
		{
			var entity = new AttributeEntity();
			var entityMapper = new EntityMapper<AttributeEntity>();
			var mutator = new EntityAttributeMutator<AttributeEntity>();

			mutator.MutateEntity(entity, MutatorType.Insert, entityMapper);
			
			Assert.IsTrue(DateTime.MinValue < entity.UpdatedDate);
		}

		[TestMethod]
		public void OnUpdate()
		{
			var entity = new AttributeEntity();
			var entityMapper = new EntityMapper<AttributeEntity>();
			var mutator = new EntityAttributeMutator<AttributeEntity>();

			mutator.MutateEntity(entity, MutatorType.Update, entityMapper);
			
			Assert.IsTrue(DateTime.MinValue < entity.UpdatedDate);
		}
	}
}
