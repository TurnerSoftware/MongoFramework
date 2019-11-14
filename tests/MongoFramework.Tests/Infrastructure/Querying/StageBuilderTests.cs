using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure.Querying;

namespace MongoFramework.Tests.Infrastructure.Querying
{
	[TestClass]
	public class StageBuilderTests : QueryTestBase
	{
		[TestMethod]
		public void EmptyQueryableHasNoStages()
		{
			var expression = GetExpression(q => q);
			var stages = StageBuilder.BuildFromExpression(expression);
			Assert.AreEqual(0, stages.Count());
		}
	}
}
