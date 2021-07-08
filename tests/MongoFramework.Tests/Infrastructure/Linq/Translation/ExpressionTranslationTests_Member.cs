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
	public class ExpressionTranslationTests_Member : QueryTestBase
	{
		[TestMethod]
		public void TranslateMember_SingleLevelMember()
		{
			var expression = GetTransform(e => e.SingleString);
			var result = ExpressionTranslation.TranslateMember(expression);
			var expected = new BsonString("SingleString");
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TranslateMember_MultiLevelMember()
		{
			var expression = GetTransform(e => e.SingleModel.SingleNumber);
			var result = ExpressionTranslation.TranslateMember(expression);
			var expected = new BsonString("SingleModel.SingleNumber");
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TranslateMember_MemberWithArrayIndex_AtStart()
		{
			var expression = GetTransform(e => e.ArrayOfModels[3].SingleNumber);
			var result = ExpressionTranslation.TranslateMember(expression);
			var expected = new BsonString("ArrayOfModels.3.SingleNumber");
			Assert.AreEqual(expected, result);
		}
		[TestMethod]
		public void TranslateMember_MemberWithArrayIndex_AtEnd()
		{
			var expression = GetTransform(e => e.SingleModel.ArrayOfModels[2]);
			var result = ExpressionTranslation.TranslateMember(expression);
			var expected = new BsonString("SingleModel.ArrayOfModels.2");
			Assert.AreEqual(expected, result);
		}
	}
}
