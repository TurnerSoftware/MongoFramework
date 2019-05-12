using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoFramework.Attributes;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class IndexProcessor : IMappingProcessor
	{
		public void ApplyMapping(IEntityDefinition definition, BsonClassMap classMap)
		{
			definition.Indexes = definition.TraverseProperties().SelectMany(p =>
				p.PropertyInfo.GetCustomAttributes<IndexAttribute>().Select(a => new EntityIndex
				{
					Property = p,
					IndexName = a.Name,
					IsUnique = a.IsUnique,
					SortOrder = a.SortOrder,
					IndexPriority = a.IndexPriority
				})
			);
		}
	}
}
