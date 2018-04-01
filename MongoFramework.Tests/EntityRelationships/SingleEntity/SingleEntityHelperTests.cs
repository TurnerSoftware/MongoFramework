using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure.EntityRelationships;

namespace MongoFramework.Tests.EntityRelationships.SingleEntity
{
	[TestClass]
	public class SingleEntityHelperTests
	{
		[TestMethod]
		public void IdentifyAllRelationships()
		{
			var relationships = EntityRelationshipHelper.GetRelationshipsForType(typeof(BaseEntityModel));
			Assert.AreEqual(2, relationships.Count());
		}

		[TestMethod]
		public void ForeignKeyAttributeOnId()
		{
			var relationships = EntityRelationshipHelper.GetRelationshipsForType(typeof(BaseEntityModel));

			var createdByIdProperty = typeof(BaseEntityModel).GetProperty("CreatedById");
			var createdByNavigationProperty = typeof(BaseEntityModel).GetProperty("CreatedBy");
			var attributeOnIdRelationship = relationships.Where(r => r.IdProperty == createdByIdProperty).FirstOrDefault();

			Assert.AreEqual(createdByNavigationProperty, attributeOnIdRelationship.NavigationProperty);
			Assert.IsFalse(attributeOnIdRelationship.IsCollection);
		}

		[TestMethod]
		public void ForeignKeyAttributeOnNavigationProperty()
		{
			var relationships = EntityRelationshipHelper.GetRelationshipsForType(typeof(BaseEntityModel));

			var updatedByIdProperty = typeof(BaseEntityModel).GetProperty("UpdatedById");
			var updatedByNavigationProperty = typeof(BaseEntityModel).GetProperty("UpdatedBy");
			var attributeOnIdRelationship = relationships.Where(r => r.IdProperty == updatedByIdProperty).FirstOrDefault();

			Assert.AreEqual(updatedByNavigationProperty, attributeOnIdRelationship.NavigationProperty);
			Assert.IsFalse(attributeOnIdRelationship.IsCollection);
		}
	}
}
