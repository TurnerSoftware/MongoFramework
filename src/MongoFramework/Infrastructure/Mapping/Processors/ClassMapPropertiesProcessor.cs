using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MongoDB.Bson.Serialization;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class ClassMapPropertiesProcessor : IMappingProcessor
	{
		public void ApplyMapping(IEntityDefinition definition, BsonClassMap classMap)
		{
			definition.Properties = classMap.DeclaredMemberMaps.Select(m => new EntityProperty
			{
				EntityType = definition.EntityType,
				IsKey = m == classMap.IdMemberMap,
				ElementName = m.ElementName,
				FullPath = m.ElementName,
				PropertyType = (m.MemberInfo as PropertyInfo).PropertyType,
				PropertyInfo = m.MemberInfo as PropertyInfo
			});
		}
	}
}
