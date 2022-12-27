using System.Collections.Generic;
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoFramework.Attributes;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class ExtraElementsProcessor : IMappingProcessor
	{
		public void ApplyMapping(IEntityDefinition definition)
		{
			var entityType = definition.EntityType;

			//Ignore extra elements when the "IgnoreExtraElementsAttribute" is on the Entity
			var ignoreExtraElements = entityType.GetCustomAttribute<IgnoreExtraElementsAttribute>();
			if (ignoreExtraElements != null)
			{
				definition.ExtraElements = new EntityExtraElementsDefinition
				{
					IgnoreExtraElements = true,
					IgnoreInherited = ignoreExtraElements.IgnoreInherited
				};
			}
			else
			{
				//If any of the Entity's properties have the "ExtraElementsAttribute", assign that against the BsonClassMap

				foreach (var property in definition.Properties)
				{
					var extraElementsAttribute = property.PropertyInfo.GetCustomAttribute<ExtraElementsAttribute>();
					if (extraElementsAttribute != null && typeof(IDictionary<string, object>).IsAssignableFrom(property.PropertyType))
					{
						definition.ExtraElements = new EntityExtraElementsDefinition
						{
							Property = property,
							IgnoreExtraElements = false
						};
					}
				}
			}
		}
	}
}
