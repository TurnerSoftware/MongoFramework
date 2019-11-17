using System;
using System.Collections.Generic;
using System.Text;
using MongoFramework.Infrastructure.Querying.Translators;

namespace MongoFramework.Infrastructure.Querying
{
	public static class DefaultTranslators
	{
		public static void AddTranslators()
		{
			ExpressionTranslation.AddTranslator(new WhereTranslator());
			ExpressionTranslation.AddTranslator(new OrderByTranslator());
			ExpressionTranslation.AddTranslator(new SelectTranslator());
		}
	}
}
