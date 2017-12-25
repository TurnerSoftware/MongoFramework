using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Attributes;
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

namespace MongoFramework.Tests.Mutation.Mutators.IncrementNumber
{
	public class IncrementalEntity
	{
		[IncrementNumber]
		public int ByDefault { get; set; }
		[IncrementNumber(true)]
		public int ByUpdateOnly { get; set; }
		[IncrementNumber(10)]
		public int ByTen { get; set; }
	}

	[TestClass]
	public class IncrementNumberMutatorTests
	{
		private IncrementalEntity Mutate(MutatorType type, IncrementalEntity entity = null)
		{
			entity = entity ?? new IncrementalEntity();
			var entityMapper = new EntityMapper<IncrementalEntity>();
			var mutator = new EntityAttributeMutator<IncrementalEntity>();

			mutator.MutateEntity(entity, type, entityMapper);
			return entity;
		}

		[TestMethod]
		public void OnDefaultInsert()
		{
			var entity = Mutate(MutatorType.Insert);
			Assert.AreEqual(1, entity.ByDefault);
		}

		[TestMethod]
		public void OnDefaultUpdate()
		{
			var entity = Mutate(MutatorType.Update);
			Assert.AreEqual(1, entity.ByDefault);
		}

		[TestMethod]
		public void OnUpdateMultipleTimes()
		{
			var entity = Mutate(MutatorType.Update);
			Assert.AreEqual(1, entity.ByDefault);
			entity = Mutate(MutatorType.Update, entity);
			Assert.AreEqual(2, entity.ByDefault);
			entity = Mutate(MutatorType.Update, entity);
			Assert.AreEqual(3, entity.ByDefault);
		}

		[TestMethod]
		public void IncrementOnUpdateOnly()
		{
			var entity = Mutate(MutatorType.Insert);
			Assert.AreEqual(0, entity.ByUpdateOnly);
			entity = Mutate(MutatorType.Update);
			Assert.AreEqual(1, entity.ByUpdateOnly);
		}

		public void IncrementByTen()
		{
			var entity = Mutate(MutatorType.Insert);
			Assert.AreEqual(10, entity.ByTen);
		}
	}
}
