using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MongoFramework.Infrastructure.Linq
{
	public static class ExpressionAccumulatorHelper
	{
		public static LambdaExpression AsyncResultTransformer(LambdaExpression resultTransformer)
		{
			var resultTransformerType = resultTransformer.Body;

			return null;
		}
	}
}
