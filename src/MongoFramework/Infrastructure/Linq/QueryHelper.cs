using System.Linq;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Infrastructure.Linq
{
	public static class QueryHelper
	{
		public static string GetQuery<TEntity>(AggregateExecutionModel model)
		{
			var entityDefinition = EntityMapping.GetOrCreateDefinition(typeof(TEntity));
			var stages = string.Join(", ", model.Stages.Select(x => x.ToString()));
			return $"db.{entityDefinition.CollectionName}.aggregate([{stages}])";
		}
	}
}
