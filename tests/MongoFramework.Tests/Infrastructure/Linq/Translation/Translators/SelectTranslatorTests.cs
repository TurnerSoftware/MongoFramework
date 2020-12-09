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
	public class SelectorTranslatorTests : QueryTestBase
	{
		[TestMethod]
		public void SelectProperty()
		{
			var expression = GetExpression(q => q.Select(e => e.Id));
			var result = new SelectTranslator().TranslateMethod(expression as MethodCallExpression);
			var expected = new BsonDocument
			{
				{
					"$project",
					new BsonDocument
					{
						{ "Id", "$Id" },
						{ "_id", 0 }
					}
				}
			};

			Assert.AreEqual(expected, result);
		}


		[TestMethod]
		public void SelectNewAnonymousType()
		{
			var expression = GetExpression(q => q.Select(e => new { CustomPropertyName = e.Id }));
			var result = new SelectTranslator().TranslateMethod(expression as MethodCallExpression);
			var expected = new BsonDocument
			{
				{
					"$project",
					new BsonDocument
					{
						{ "CustomPropertyName", "Id" },
						{ "_id", 0 }
					}
				}
			};

			Assert.AreEqual(expected, result);
		}
	}
}
