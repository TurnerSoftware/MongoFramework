using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.EntityRelationships;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mutation.Mutators;
using MongoFramework.Infrastructure.Mutation;
using System;
using System.Linq;

namespace MongoFramework.Tests.EntityRelationships.SingleEntity
{
	[TestClass]
	public class SingleEntityTests : DbTest
	{
		[TestMethod]
		public void SaveNewEntity()
		{
			var database = TestConfiguration.GetDatabase();
			var baseEntity = new BaseEntityModel
			{
				CreatedBy = new UserEntityModel
				{
					Username = "SaveNewEntity-CreatedBy"
				}
			};

			var entityMapper = new EntityMapper<BaseEntityModel>();
			var entityRelationshipWriter = new EntityRelationshipWriter<BaseEntityModel>(database, entityMapper);

			entityRelationshipWriter.CommitEntityRelationships(new[] { baseEntity });

			Assert.IsNotNull(baseEntity.CreatedById);
			Assert.IsTrue(baseEntity.CreatedById == baseEntity.CreatedBy.Id);
		}

		[TestMethod]
		public void LoadEntity()
		{
			var database = TestConfiguration.GetDatabase();

			var userEntity = new UserEntityModel
			{
				Username = "LoadEntity-UpdatedBy"
			};
			var dbEntityWriter = new DbEntityWriter<UserEntityModel>(database);
			var collection = new DbEntityCollection<UserEntityModel>
			{
				userEntity
			};
			dbEntityWriter.Write(collection);

			var baseEntity = new BaseEntityModel
			{
				UpdatedById = userEntity.Id
			};

			var entityMapper = new EntityMapper<BaseEntityModel>();
			new NavigationPropertyMutator<BaseEntityModel>().MutateEntity(baseEntity, MutatorType.Select, entityMapper, database);

			Assert.AreEqual("LoadEntity-UpdatedBy", baseEntity.UpdatedBy.Username);
		}
	}
}
