﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoFramework.Attributes;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class ExtraElementsProcessor : IMappingProcessor
	{
		public void ApplyMapping(Type entityType, BsonClassMap classMap)
		{
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
				var extraElementsProperty = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
					.Select(p => new
					{
						PropertyInfo = p,
						ExtraElementsAttribute = p.GetCustomAttribute<ExtraElementsAttribute>()
					}).Where(p => p.ExtraElementsAttribute != null).FirstOrDefault();

				if (extraElementsProperty != null && typeof(IDictionary<string, object>).IsAssignableFrom(extraElementsProperty.PropertyInfo.PropertyType))
				{
					classMap.SetExtraElementsMember(new BsonMemberMap(classMap, extraElementsProperty.PropertyInfo));
				}
			}
		}
	}
}
