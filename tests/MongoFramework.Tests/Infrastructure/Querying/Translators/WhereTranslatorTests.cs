using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoFramework.Infrastructure.Querying.Translators;

namespace MongoFramework.Tests.Infrastructure.Querying.Translators
{
	[TestClass]
	public class WhereTranslatorTests : QueryTestBase
	{
		[TestMethod]
		public void WrapsConditionalStatement()
		{
			var expression = GetExpression(q => q.Where(e => e.Id == ""));
			var result = new WhereTranslator().TranslateMethod(expression as MethodCallExpression);
			var expected = new BsonDocument
			{
				{
					"$match",
					new BsonDocument
					{
						{ "Id", new BsonDocument { { "$eq", "" } } }
					}
				}
			};

			Assert.AreEqual(expected, result);
		}
	}
}
