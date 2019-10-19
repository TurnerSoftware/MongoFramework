using MongoDB.Bson.Serialization;
using MongoFramework.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
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
				//If any of the Entity's properties have the "ExtraElementsAttribute", assign that against the BsonClassMap
				var extraElementsProperty = definition.Properties
					.Select(p => new
					{
						Property = p,
						ExtraElementsAttribute = p.PropertyInfo.GetCustomAttribute<ExtraElementsAttribute>()
					}).Where(p => p.ExtraElementsAttribute != null).FirstOrDefault();

				if (extraElementsProperty != null && typeof(IDictionary<string, object>).IsAssignableFrom(extraElementsProperty.Property.PropertyType))
				{
					var memberMap = classMap.DeclaredMemberMaps
						.Where(m => m.ElementName == extraElementsProperty.Property.ElementName)
						.FirstOrDefault();
					classMap.SetExtraElementsMember(memberMap);
				}
			}
		}
	}
}
