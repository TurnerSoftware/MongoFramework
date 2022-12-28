using System.Collections.Generic;
using System.Reflection;
using MongoFramework.Attributes;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class IndexProcessor : IMappingProcessor
	{
		public void ApplyMapping(IEntityDefinition definition)
		{
			var definitionIndexes = new List<EntityIndexDefinition>();
			foreach (var property in definition.TraverseProperties())
			{
				foreach (var indexAttribute in property.Property.PropertyInfo.GetCustomAttributes<IndexAttribute>())
				{
					definitionIndexes.Add(new EntityIndexDefinition
					{
						Property = property.Property,
						Path = property.GetPath(),
						IndexName = indexAttribute.Name,
						IsUnique = indexAttribute.IsUnique,
						SortOrder = indexAttribute.SortOrder,
						IndexPriority = indexAttribute.IndexPriority,
						IndexType = indexAttribute.IndexType,
						IsTenantExclusive = indexAttribute.IsTenantExclusive
					});
				}
			}

			definition.Indexes = definitionIndexes;
		}
	}
}
