using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MongoFramework.Infrastructure.Mutators;
using MongoFramework.Attributes;

namespace MongoFramework.Infrastructure
{
	public class DbEntityMapper : IDbEntityMapper
	{
		private Type EntityType { get; set; }
		private BsonClassMap ClassMap { get; set; }

		private IEnumerable<BsonClassMap> ClassMapHierarchyCache { get; set; }

		public DbEntityMapper(Type entityType)
		{
			EntityType = entityType ?? throw new ArgumentNullException("entityType");
			InitialiseClassMap();
		}

		private void InitialiseClassMap()
		{
			//For reasons unknown to me, you can't just call "BsonClassMap.LookupClassMap" as that "freezes" the class map
			//Instead, you must do the lookup and initial creation yourself.
			var classMaps = BsonClassMap.GetRegisteredClassMaps();
			ClassMap = classMaps.Where(cm => cm.ClassType == EntityType).FirstOrDefault() as BsonClassMap;

			if (ClassMap == null)
			{
				ClassMap = new BsonClassMap(EntityType);
				ClassMap.AutoMap();
				BsonClassMap.RegisterClassMap(ClassMap);
				ConfigureEntity();
			}
		}

		private void ConfigureEntity()
		{
			if (ClassMap.IsFrozen)
			{
				return;
			}

			ConfigureHierarchy();
			ConfigureEntityId();
			ConfigureMappedProperties();
			ConfigureExtraElements();
		}

		private void ConfigureHierarchy()
		{
			if (EntityType.BaseType != typeof(object))
			{
				new DbEntityMapper(EntityType.BaseType);
			}
		}

		private void ConfigureEntityId()
		{
			if (ClassMap.IsFrozen)
			{
				return;
			}

			//If no Id member map exists, find the first property with the "Key" attribute or is named "Id" and use that
			if (ClassMap.IdMemberMap == null)
			{
				var properties = EntityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				var idProperty = properties.Where(p => p.GetCustomAttribute<KeyAttribute>() != null || p.Name == "Id").FirstOrDefault();
				if (idProperty != null)
				{
					ClassMap.MapIdMember(idProperty);
				}
			}

			//If there is no Id generator, set a default based on the member type
			if (ClassMap.IdMemberMap != null && ClassMap.IdMemberMap.IdGenerator == null)
			{
				var idMemberMap = ClassMap.IdMemberMap;
				var memberType = BsonClassMap.GetMemberInfoType(idMemberMap.MemberInfo);
				if (memberType == typeof(string))
				{
					idMemberMap.SetIdGenerator(StringObjectIdGenerator.Instance);
				}
				else if (memberType == typeof(Guid))
				{
					idMemberMap.SetIdGenerator(CombGuidGenerator.Instance);
				}
				else if (memberType == typeof(ObjectId))
				{
					idMemberMap.SetIdGenerator(ObjectIdGenerator.Instance);
				}
			}
		}

		private void ConfigureMappedProperties()
		{
			if (ClassMap.IsFrozen)
			{
				return;
			}

			var properties = EntityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			foreach (var property in properties)
			{
				//Unmap fields with the "NotMappedAttribute"
				var notMappedAttribute = property.GetCustomAttribute<NotMappedAttribute>();
				if (notMappedAttribute != null)
				{
					ClassMap.UnmapProperty(property.Name);
					continue;
				}

				//Remap fields with the "ColumnAttribute"
				var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
				if (columnAttribute != null)
				{
					var mappedName = columnAttribute.Name;
					var memberMap = ClassMap.GetMemberMap(property.Name);
					memberMap.SetElementName(mappedName);
				}

				//Map the DeclaredType of any properties where the PropertyType is a class
				if (property.PropertyType.IsClass && property.PropertyType != EntityType)
				{
					new DbEntityMapper(property.PropertyType);
				}
			}
		}

		private void ConfigureExtraElements()
		{
			if (ClassMap.IsFrozen)
			{
				return;
			}

			//Ignore extra elements when the "IgnoreExtraElementsAttribute" is on the Entity
			var ignoreExtraElements = EntityType.GetCustomAttribute<IgnoreExtraElementsAttribute>();
			if (ignoreExtraElements != null)
			{
				ClassMap.SetIgnoreExtraElements(true);
				ClassMap.SetIgnoreExtraElementsIsInherited(ignoreExtraElements.IgnoreInherited);
			}
			else
			{
				//If any of the Entity's properties have the "ExtraElementsAttribute", assign that against the BsonClassMap
				var extraElementsProperty = EntityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
					.Select(p => new
					{
						PropertyInfo = p,
						ExtraElementsAttribute = p.GetCustomAttribute<ExtraElementsAttribute>()
					}).Where(p => p.ExtraElementsAttribute != null).FirstOrDefault();

				if (extraElementsProperty != null && typeof(IDictionary<object, object>).IsAssignableFrom(extraElementsProperty.PropertyInfo.PropertyType))
				{
					ClassMap.SetExtraElementsMember(new BsonMemberMap(ClassMap, extraElementsProperty.PropertyInfo));
				}
			}
		}

		private IEnumerable<BsonClassMap> GetClassMapChain()
		{
			if (ClassMapHierarchyCache != null)
			{
				return ClassMapHierarchyCache;
			}
			else
			{
				var results = new List<BsonClassMap> { ClassMap };
				var classMaps = BsonClassMap.GetRegisteredClassMaps();
				var currentType = EntityType.BaseType;
				while (currentType != typeof(object))
				{
					var currentClassMap = classMaps.Where(c => c.ClassType == currentType).FirstOrDefault();
					results.Add(currentClassMap);
					currentType = currentType.BaseType;
				}
				ClassMapHierarchyCache = results;
				return results;
			}
		}

		public string GetCollectionName()
		{
			var tableAttribute = EntityType.GetCustomAttribute<TableAttribute>();
			if (tableAttribute != null)
			{
				if (string.IsNullOrEmpty(tableAttribute.Schema))
				{
					return tableAttribute.Name;
				}
				else
				{
					return tableAttribute.Schema + "." + tableAttribute.Name;
				}
			}
			else
			{
				return EntityType.Name;
			}
		}

		public string GetIdName()
		{
			var idMemberMap = GetClassMapChain().Where(c => c.IdMemberMap != null).Select(c => c.IdMemberMap).FirstOrDefault();
			if (idMemberMap != null)
			{
				return idMemberMap.MemberName;
			}
			return null;
		}

		public object GetIdValue(object entity)
		{
			var idMemberMap = GetClassMapChain().Where(c => c.IdMemberMap != null).Select(c => c.IdMemberMap).FirstOrDefault();
			if (idMemberMap != null)
			{
				if (idMemberMap.MemberInfo is PropertyInfo propertyInfo)
				{
					return propertyInfo.GetValue(entity);
				}
			}
			return null;
		}

		public IEnumerable<PropertyInfo> GetMappedProperties()
		{
			return GetMappedProperties(true);
		}

		public IEnumerable<PropertyInfo> GetMappedProperties(bool includeInherited)
		{
			if (includeInherited && EntityType.BaseType != typeof(object))
			{
				var declaredProperties = GetMappedProperties(false);
				var inheritedProperties = new DbEntityMapper(EntityType.BaseType).GetMappedProperties();
				return declaredProperties.Concat(inheritedProperties);
			}
			else
			{
				return ClassMap.DeclaredMemberMaps.Select(m => m.MemberInfo as PropertyInfo);
			}
		}
	}

	public class DbEntityMapper<TEntity> : DbEntityMapper
	{
		public DbEntityMapper() : base(typeof(TEntity)) { }
	}
}
