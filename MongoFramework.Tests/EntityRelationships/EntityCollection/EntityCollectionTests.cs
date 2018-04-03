using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.EntityRelationships;

namespace MongoFramework.Tests.EntityRelationships.EntityCollection
{
	[TestClass]
	public class EntityCollectionTests : TestBase
	{
		[TestMethod]
		public void AddItemsToExistingEntity()
		{
			var database = TestConfiguration.GetDatabase();
			var baseEntity = new BaseEntityModel
			{
				Description = "1"
			};

			var dbEntityWriter = new DbEntityWriter<BaseEntityModel>(database);
			var collection = new DbEntityCollection<BaseEntityModel>
			{
				baseEntity
			};
			dbEntityWriter.Write(collection);

			var dbEntityReader = new DbEntityReader<BaseEntityModel>(database);
			var dbEntity = dbEntityReader.AsQueryable().Where(e => e.Id == baseEntity.Id).FirstOrDefault();

			dbEntity.RelatedEntities.Add(new RelatedEntityModel
			{
				Description = "AddItemsToExistingEntity-RelatedEntityModel-1"
			});

			collection.Clear();
			collection.Update(dbEntity, DbEntityEntryState.Updated);
			dbEntityWriter.Write(collection);

			Assert.IsNotNull(dbEntity.RelatedEntities.FirstOrDefault().Id);
			

			var relationships = EntityRelationshipHelper.GetRelationshipsForType(typeof(BaseEntityModel));
		}
	}
}
