using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure.Commands;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Tests.Infrastructure.Mapping
{
	[TestClass]
	public class EntityDefinitionExtensionTests : TestBase
	{
		public class IdNameParentModel
		{
			public string Id { get; set; }
		}

		public class IdNameChildModel : IdNameParentModel
		{
			public string Description { get; set; }
		}

		public class OverridePropertyBaseModel
		{
			public virtual string TargetProperty { get; set; }
		}

		public class OverridePropertyChildModel : OverridePropertyBaseModel
		{
			public override string TargetProperty { get; set; }
		}

		public class OverridePropertyGrandChildModel : OverridePropertyChildModel
		{

		}

		public class TenantModel : IHaveTenantId
		{
			public string Id { get; set; }
			public string TenantId { get; set; }
		}

		[TestMethod]
		public void GetIdNameChecksInheritence()
		{
			var definition = EntityMapping.RegisterType(typeof(IdNameChildModel));
			var parentDefinition = EntityMapping.GetOrCreateDefinition(typeof(IdNameParentModel));

			Assert.AreEqual("Id", definition.GetIdName());
			Assert.AreEqual("Id", parentDefinition.GetIdName());
		}

		[TestMethod]
		public void GetInheritedPropertiesTakesBaseProperties()
		{
			var definition = EntityMapping.RegisterType(typeof(OverridePropertyGrandChildModel));
			var inheritedProperties = definition.GetInheritedProperties().ToArray();
			Assert.AreEqual(1, inheritedProperties.Length);
			Assert.AreEqual(typeof(OverridePropertyBaseModel), inheritedProperties[0].EntityDefinition.EntityType);
		}
		[TestMethod]
		public void GetAllPropertiesTakesBaseProperties()
		{
			var definition = EntityMapping.RegisterType(typeof(OverridePropertyChildModel));
			var allProperties = definition.GetAllProperties().ToArray();
			Assert.AreEqual(1, allProperties.Length);
			Assert.AreEqual(typeof(OverridePropertyBaseModel), allProperties[0].EntityDefinition.EntityType);
		}

		[TestMethod]
		public void GetTenantModelIdRequiresTenant()
		{
			var definition = EntityMapping.RegisterType(typeof(TenantModel));
			Assert.ThrowsException<ArgumentException>(() => definition.CreateIdFilter<TenantModel>("id"));
		}

	}
}
