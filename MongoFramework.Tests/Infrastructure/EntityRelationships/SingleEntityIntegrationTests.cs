using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.EntityRelationships;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mutation;
using MongoFramework.Infrastructure.Mutation.Mutators;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MongoFramework.Tests.EntityRelationships
{
	public class SingleEntityIntegrationModel
	{
		public string Id { get; set; }

		[ForeignKey("RelatedItem")]
		public string RelatedItemId { get; set; }
		public StringIdModel RelatedItem { get; set; }
	}

	[TestClass]
	public class SingleEntityIntegrationTests : TestBase
	{
		[TestMethod]
		public void AddRelationshipToNewEntity()
		{
			var database = TestConfiguration.GetDatabase();
			var entity = new SingleEntityIntegrationModel
			{
				RelatedItem = new StringIdModel
				{
					Description = "SaveNewEntity-RelatedItem"
				}
			};

			var entityMapper = new EntityMapper<SingleEntityIntegrationModel>();
			var entityRelationshipWriter = new EntityRelationshipWriter<SingleEntityIntegrationModel>(database, entityMapper);

			entityRelationshipWriter.CommitEntityRelationships(new[] { entity });

			Assert.IsNotNull(entity.RelatedItemId);
			Assert.IsTrue(entity.RelatedItemId == entity.RelatedItem.Id);
		}

		[TestMethod]
		public async Task AddRelationshipToNewEntityAsync()
		{
			var database = TestConfiguration.GetDatabase();
			var entity = new SingleEntityIntegrationModel
			{
				RelatedItem = new StringIdModel
				{
					Description = "SaveNewEntity-RelatedItem"
				}
			};

			var entityMapper = new EntityMapper<SingleEntityIntegrationModel>();
			var entityRelationshipWriter = new EntityRelationshipWriter<SingleEntityIntegrationModel>(database, entityMapper);

			await entityRelationshipWriter.CommitEntityRelationshipsAsync(new[] { entity }).ConfigureAwait(false);

			Assert.IsNotNull(entity.RelatedItemId);
			Assert.IsTrue(entity.RelatedItemId == entity.RelatedItem.Id);
		}

		[TestMethod]
		public void LoadRelationship()
		{
			var database = TestConfiguration.GetDatabase();

			var relatedEntity = new StringIdModel
			{
				Description = "LoadRelationship-RelatedItem"
			};
			var dbEntityWriter = new DbEntityWriter<StringIdModel>(database);
			var collection = new DbEntityCollection<StringIdModel>
			{
				relatedEntity
			};
			dbEntityWriter.Write(collection);

			var entity = new SingleEntityIntegrationModel
			{
				RelatedItemId = relatedEntity.Id
			};

			var entityMapper = new EntityMapper<SingleEntityIntegrationModel>();
			new NavigationPropertyMutator<SingleEntityIntegrationModel>().MutateEntity(entity, MutatorType.Select, entityMapper, database);

			Assert.AreEqual("LoadRelationship-RelatedItem", entity.RelatedItem.Description);
		}
	}
}
