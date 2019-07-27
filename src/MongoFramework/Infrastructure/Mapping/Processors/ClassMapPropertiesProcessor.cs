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
			//Remove invalid maps for the class
			foreach (var map in classMap.DeclaredMemberMaps.ToArray())
			{
				if (map.MemberInfo is PropertyInfo propertyInfo)
				{
					var getPropertyBaseDefinition = propertyInfo.GetMethod.GetBaseDefinition();
					if (getPropertyBaseDefinition.DeclaringType != definition.EntityType)
					{
						//Remove any member map that just overrides another member
						classMap.UnmapMember(map.MemberInfo);
						continue;
					}
				}
				else
				{
					//Removes any member map that isn't for a property
					classMap.UnmapMember(map.MemberInfo);
				}
			}

			definition.Properties = classMap.DeclaredMemberMaps
				.Select(m => new EntityProperty
				{
					EntityType = definition.EntityType,
					IsKey = m == classMap.IdMemberMap,
					ElementName = m.ElementName,
					FullPath = m.ElementName,
					PropertyType = (m.MemberInfo as PropertyInfo).PropertyType,
					PropertyInfo = (m.MemberInfo as PropertyInfo)
				});
		}
	}
}
