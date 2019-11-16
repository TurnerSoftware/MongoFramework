using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoFramework.Infrastructure.Querying;
using MongoFramework.Infrastructure.Querying.Translators;

namespace MongoFramework.Tests.Infrastructure.Querying
{
	[TestClass]
	public class ExpressionTranslationTests : QueryTestBase
	{
		[TestMethod]
		public void TranslateConditional_Equals()
		{
			var expression = GetConditional(e => e.Id == "");
			var result = ExpressionTranslation.TranslateConditional(expression);
			var expected = new BsonDocument
			{
				{ "Id", new BsonDocument { { "$eq", "" } } }
			};
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TranslateConditional_NotEquals()
		{
			var expression = GetConditional(e => e.Id != "");
			var result = ExpressionTranslation.TranslateConditional(expression);
			var expected = new BsonDocument
			{
				{ "Id", new BsonDocument { { "$nq", "" } } }
			};
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TranslateConditional_LessThan()
		{
			var expression = GetConditional(e => e.SingleNumber < 5);
			var result = ExpressionTranslation.TranslateConditional(expression);
			var expected = new BsonDocument
			{
				{ "SingleNumber", new BsonDocument { { "$lt", 5 } } }
			};
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TranslateConditional_GreaterThan()
		{
			var expression = GetConditional(e => e.SingleNumber > 5);
			var result = ExpressionTranslation.TranslateConditional(expression);
			var expected = new BsonDocument
			{
				{ "SingleNumber", new BsonDocument { { "$gt", 5 } } }
			};
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TranslateConditional_LessThanOrEqual()
		{
			var expression = GetConditional(e => e.SingleNumber <= 5);
			var result = ExpressionTranslation.TranslateConditional(expression);
			var expected = new BsonDocument
			{
				{ "SingleNumber", new BsonDocument { { "$lte", 5 } } }
			};
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TranslateConditional_GreaterThanOrEqual()
		{
			var expression = GetConditional(e => e.SingleNumber >= 5);
			var result = ExpressionTranslation.TranslateConditional(expression);
			var expected = new BsonDocument
			{
				{ "SingleNumber", new BsonDocument { { "$gte", 5 } } }
			};
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TranslateConditional_AndAlso()
		{
			var expression = GetConditional(e => e.Id == "" && e.SingleNumber >= 5);
			var result = ExpressionTranslation.TranslateConditional(expression);
			var expected = new BsonDocument
			{
				{ "Id", new BsonDocument { { "$eq", "" } } },
				{ "SingleNumber", new BsonDocument { { "$gte", 5 } } }
			};
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TranslateConditional_OrElse()
		{
			var expression = GetConditional(e => e.Id == "" || e.SingleNumber >= 5);
			var result = ExpressionTranslation.TranslateConditional(expression);
			var expected = new BsonDocument
			{
				{ 
					"$or",
					new BsonArray
					{
						new BsonDocument { { "Id", new BsonDocument { { "$eq", "" } } } },
						new BsonDocument { { "SingleNumber", new BsonDocument { { "$gte", 5 } } } }
					}
				}
			};
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TranslateConditional_Not_AndAlso()
		{
			var expression = GetConditional(e => !(e.Id == "" && e.SingleNumber >= 5));
			var result = ExpressionTranslation.TranslateConditional(expression);
			var expected = new BsonDocument
			{
				{ "Id", new BsonDocument { { "$not", new BsonDocument { { "$eq", "" } } } } },
				{ "SingleNumber", new BsonDocument { { "$not", new BsonDocument { { "$gte", 5 } } } } }
			};
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TranslateConditional_Not_OrElse()
		{
			var expression = GetConditional(e => !(e.Id == "" || e.SingleNumber >= 5));
			var result = ExpressionTranslation.TranslateConditional(expression);
			var expected = new BsonDocument
			{
				{
					"$nor",
					new BsonArray
					{
						new BsonDocument { { "Id", new BsonDocument { { "$eq", "" } } } },
						new BsonDocument { { "SingleNumber", new BsonDocument { { "$gte", 5 } } } }
					}
				}
			};
			Assert.AreEqual(expected, result);
		}
	}
}
