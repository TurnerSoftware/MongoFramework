using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoFramework.Infrastructure.Linq.Translation;

namespace MongoFramework.Tests.Infrastructure.Linq.Translation
{
	[TestClass]
	public class ExpressionTranslationTests_Instantiation : QueryTestBase
	{
		[TestMethod]
		public void TranslateInstantiation_Anonymous()
		{
			var expression = GetTransform(e => new
			{
				e.Id,
				MyNumber = e.SingleNumber
			});
			var result = ExpressionTranslation.TranslateInstantiation(expression);
			var expected = new BsonDocument
			{
				{ "Id", "Id" },
				{ "MyNumber", "SingleNumber" }
			};
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TranslateInstantiation_RealType()
		{
			var expression = GetTransform(e => new QueryTestModel
			{
				Id = e.Id,
				SingleNumber = e.SingleNumber
			});
			var result = ExpressionTranslation.TranslateInstantiation(expression);
			var expected = new BsonDocument
			{
				{ "Id", "Id" },
				{ "SingleNumber", "SingleNumber" }
			};
			Assert.AreEqual(expected, result);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void TranslateInstantiation_InvalidExpression()
		{
			var expression = GetTransform(e => e.SingleNumber);
			ExpressionTranslation.TranslateInstantiation(expression);
		}
	}
}
