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

namespace MongoFramework.Tests.Infrastructure.EntityRelationships
{
	[TestClass]
	public class SingleEntityIntegrationTests : TestBase
	{
		public class SingleEntityIntegrationModel
		{
			public string Id { get; set; }

			[ForeignKey("RelatedItem")]
			public string RelatedItemId { get; set; }
			public StringIdModel RelatedItem { get; set; }
		}

		[TestMethod]
		public void AddRelationshipToNewEntity()
		{
			var connection = TestConfiguration.GetConnection();
			var entity = new SingleEntityIntegrationModel
			{
				RelatedItem = new StringIdModel
				{
					Description = "SaveNewEntity-RelatedItem"
				}
			};

			var entityRelationshipWriter = new EntityRelationshipWriter<SingleEntityIntegrationModel>(connection);

			entityRelationshipWriter.CommitEntityRelationships(new[] { entity });

			Assert.IsNotNull(entity.RelatedItemId);
			Assert.IsTrue(entity.RelatedItemId == entity.RelatedItem.Id);
		}

		[TestMethod]
		public async Task AddRelationshipToNewEntityAsync()
		{
			var connection = TestConfiguration.GetConnection();
			var entity = new SingleEntityIntegrationModel
			{
				RelatedItem = new StringIdModel
				{
					Description = "SaveNewEntity-RelatedItem"
				}
			};

			var entityRelationshipWriter = new EntityRelationshipWriter<SingleEntityIntegrationModel>(connection);

			await entityRelationshipWriter.CommitEntityRelationshipsAsync(new[] { entity }).ConfigureAwait(false);

			Assert.IsNotNull(entity.RelatedItemId);
			Assert.IsTrue(entity.RelatedItemId == entity.RelatedItem.Id);
		}

		[TestMethod]
		public void LoadRelationship()
		{
			var connection = TestConfiguration.GetConnection();

			var relatedEntity = new StringIdModel
			{
				Description = "LoadRelationship-RelatedItem"
			};
			var dbEntityWriter = new EntityWriter<StringIdModel>(connection);
			var collection = new EntityCollection<StringIdModel>(connection.GetEntityMapper(typeof(StringIdModel)))
			{
				relatedEntity
			};
			dbEntityWriter.Write(collection);

			var entity = new SingleEntityIntegrationModel
			{
				RelatedItemId = relatedEntity.Id
			};

			new NavigationPropertyMutator<SingleEntityIntegrationModel>().MutateEntity(entity, MutatorType.Select, connection);

			Assert.AreEqual("LoadRelationship-RelatedItem", entity.RelatedItem.Description);
		}
		
		[TestMethod]
		public void UpdateRelationshipEntityProperties()
		{
			var connection = TestConfiguration.GetConnection();
			var entity = new SingleEntityIntegrationModel
			{
				RelatedItem = new StringIdModel
				{
					Description = "UpdateRelationshipEntity-RelatedItem"
				}
			};
			
			var entityRelationshipWriter = new EntityRelationshipWriter<SingleEntityIntegrationModel>(connection);
			entityRelationshipWriter.CommitEntityRelationships(new[] { entity });
			Assert.AreEqual("UpdateRelationshipEntity-RelatedItem", entity.RelatedItem.Description);

			entity.RelatedItem.Description = "UpdateRelationshipEntity-RelatedItem-Updated";
			entityRelationshipWriter.CommitEntityRelationships(new[] { entity });
			Assert.AreEqual("UpdateRelationshipEntity-RelatedItem-Updated", entity.RelatedItem.Description);

			var reader = new EntityReader<StringIdModel>(connection);
			var relatedItem = reader.AsQueryable().FirstOrDefault(e => e.Id == entity.RelatedItemId);
			Assert.AreEqual("UpdateRelationshipEntity-RelatedItem-Updated", relatedItem.Description);
		}
	}
}
