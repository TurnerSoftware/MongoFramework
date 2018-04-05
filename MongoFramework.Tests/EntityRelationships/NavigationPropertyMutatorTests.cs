using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure.Mutation.Mutators;

namespace MongoFramework.Tests.EntityRelationships
{
	[TestClass]
	public class NavigationPropertyMutatorTests
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void MutatorRequiresDatabase()
		{
			var mutator = new NavigationPropertyMutator<GuidIdModel>();
			mutator.MutateEntity(new GuidIdModel(), Infrastructure.Mutation.MutatorType.Create, null, null);
		}
	}
}
