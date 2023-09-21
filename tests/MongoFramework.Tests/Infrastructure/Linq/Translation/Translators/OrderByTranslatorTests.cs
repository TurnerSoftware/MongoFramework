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
	public class OrderByTranslatorTests : QueryTestBase
	{
		[TestMethod]
		public void OrderBy()
		{
			var expression = GetExpression(q => q.OrderBy(e => e.Id));
			var result = new OrderByTranslator().TranslateMethod(expression as MethodCallExpression);
			var expected = new BsonDocument
			{
				{
					"$sort",
					new BsonDocument
					{
						{ "Id", 1 }
					}
				}
			};

			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void OrderByDescending()
		{
			var expression = GetExpression(q => q.OrderByDescending(e => e.Id));
			var result = new OrderByTranslator().TranslateMethod(expression as MethodCallExpression);
			var expected = new BsonDocument
			{
				{
					"$sort",
					new BsonDocument
					{
						{ "Id", -1 }
					}
				}
			};

			Assert.AreEqual(expected, result);
		}
	}
}
