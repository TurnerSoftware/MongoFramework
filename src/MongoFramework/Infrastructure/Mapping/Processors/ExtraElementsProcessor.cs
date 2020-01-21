using MongoDB.Bson.Serialization;
using MongoFramework.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class ExtraElementsProcessor : IMappingProcessor
	{
		public void ApplyMapping(IEntityDefinition definition, BsonClassMap classMap)
		{
			var entityType = definition.EntityType;

			//Ignore extra elements when the "IgnoreExtraElementsAttribute" is on the Entity
			var ignoreExtraElements = entityType.GetCustomAttribute<IgnoreExtraElementsAttribute>();
			if (ignoreExtraElements != null)
			{
				classMap.SetIgnoreExtraElements(true);
				classMap.SetIgnoreExtraElementsIsInherited(ignoreExtraElements.IgnoreInherited);
			}
			else
			{
				classMap.SetIgnoreExtraElements(false);

				//If any of the Entity's properties have the "ExtraElementsAttribute", assign that against the BsonClassMap

				foreach (var property in definition.Properties)
				{
					var extraElementsAttribute = property.PropertyInfo.GetCustomAttribute<ExtraElementsAttribute>();
					if (extraElementsAttribute != null && typeof(IDictionary<string, object>).IsAssignableFrom(property.PropertyType))
					{
						foreach (var memberMap in classMap.DeclaredMemberMaps)
						{
							if (memberMap.ElementName == property.ElementName)
							{
								classMap.SetExtraElementsMember(memberMap);
								return;
							}
						}
					}
				}
			}
		}
	}
}
