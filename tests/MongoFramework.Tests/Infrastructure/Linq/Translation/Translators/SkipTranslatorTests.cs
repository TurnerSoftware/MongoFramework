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
	public class SkipTranslatorTests : QueryTestBase
	{
		[TestMethod]
		public void Skip()
		{
			var expression = GetExpression(q => q.Skip(5));
			var result = new SkipTranslator().TranslateMethod(expression as MethodCallExpression);
			var expected = new BsonDocument
			{
				{
					"$skip",
					5
				}
			};

			Assert.AreEqual(expected, result);
		}
	}
}
