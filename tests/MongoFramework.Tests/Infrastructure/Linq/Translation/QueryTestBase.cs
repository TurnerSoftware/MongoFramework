using MongoFramework.Infrastructure.Linq.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Tests.Infrastructure.Linq.Translation
{
	public abstract class QueryTestBase : TestBase
	{
		protected class QueryTestModel
		{
			public string Id { get; set; }
			public string SingleString { get; set; }
			public DateTime SingleDateTime { get; set; }
			public TimeSpan SingleTimeSpan { get; set; }
			public Uri SingleUri { get; set; }
			public int SingleNumber { get; set; }

			public string[] ArrayOfStrings { get; set; }
			public int[] ArrayOfNumbers { get; set; }

			public QueryTestModel SingleModel { get; set; }
			public QueryTestModel[] ArrayOfModels { get; set; }
			public IEnumerable<QueryTestModel> EnumerableOfModels { get; set; }
			public Dictionary<string, string> DictionaryOfStrings { get; set; }
		}

		protected static Expression GetExpression(Func<IQueryable<QueryTestModel>, IQueryable> query)
		{
			var queryable = Queryable.AsQueryable(Array.Empty<QueryTestModel>());
			var userQueryable = query(queryable);
			return ExpressionTranslation.UnwrapLambda(userQueryable.Expression);
		}

		protected static Expression GetConditional(Expression<Func<QueryTestModel, bool>> expression)
		{
			return ExpressionTranslation.UnwrapLambda(expression);
		}

		protected static Expression GetTransform<TResult>(Expression<Func<QueryTestModel, TResult>> expression)
		{
			return ExpressionTranslation.UnwrapLambda(expression);
		}
	}
}
