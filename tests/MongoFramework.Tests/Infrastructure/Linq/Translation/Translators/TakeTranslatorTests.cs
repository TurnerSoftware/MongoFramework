using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoFramework.Infrastructure.Linq.Translation.Translators;

namespace MongoFramework.Tests.Infrastructure.Linq.Translation.Translators
{
	[TestClass]
	public class TakeTranslatorTests : QueryTestBase
	{
		[TestMethod]
		public void Take()
		{
			var expression = GetExpression(q => q.Take(5));
			var result = new TakeTranslator().TranslateMethod(expression as MethodCallExpression);
			var expected = new BsonDocument
			{
				{
					"$limit",
					5
				}
			};

			Assert.AreEqual(expected, result);
		}
	}
}
