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
			var definitionIndexes = new List<EntityIndex>();
			foreach (var property in definition.TraverseProperties())
			{
				foreach (var indexAttribute in property.PropertyInfo.GetCustomAttributes<IndexAttribute>())
				{
					definitionIndexes.Add(new EntityIndex
					{
						Property = property,
						IndexName = indexAttribute.Name,
						IsUnique = indexAttribute.IsUnique,
						SortOrder = indexAttribute.SortOrder,
						IndexPriority = indexAttribute.IndexPriority,
						IndexType = indexAttribute.IndexType,
						IsTenantExclusive = indexAttribute.IsTenantExclusve
					});
				}
			}

			definition.Indexes = definitionIndexes;
		}
	}
}
