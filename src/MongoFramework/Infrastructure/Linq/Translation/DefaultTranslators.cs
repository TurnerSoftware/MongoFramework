using MongoFramework.Infrastructure.Linq.Translation.Translators;
using System;
using System.Collections.Generic;
using System.Text;

namespace MongoFramework.Infrastructure.Linq.Translation
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
