using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace MongoFramework.Infrastructure.Mapping
{
	public class EntityMapper : IEntityMapper
	{
		public Type EntityType { get; private set; }
		private BsonClassMap ClassMap { get; set; }
		
		private static ConcurrentDictionary<Type, IEnumerable<IEntityPropertyMap>> EntityMapCache { get; set; }

		static EntityMapper()
		{
			EntityMapCache = new ConcurrentDictionary<Type, IEnumerable<IEntityPropertyMap>>();
		}

		public EntityMapper(Type entityType)
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

				foreach (var processor in DefaultMappingPack.Instance.Processors)
				{
					processor.ApplyMapping(EntityType, ClassMap);
				}
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
			return GetEntityMapping().Where(m => m.IsKey).Select(m => m.ElementName).FirstOrDefault();
		}

		public object GetIdValue(object entity)
		{
			var idProperty = GetEntityMapping().Where(m => m.IsKey).Select(m => m.Property).FirstOrDefault();
			return idProperty?.GetValue(entity);
		}

		public IEnumerable<IEntityPropertyMap> GetEntityMapping()
		{
			return GetEntityMapping(true);
		}

		public IEnumerable<IEntityPropertyMap> GetEntityMapping(bool includeInherited)
		{
			if (includeInherited && EntityType.BaseType != typeof(object))
			{
				var declaredProperties = GetEntityMapping(false);
				var inheritedProperties = new EntityMapper(EntityType.BaseType).GetEntityMapping();
				return declaredProperties.Concat(inheritedProperties);
			}
			else
			{
				return EntityMapCache.GetOrAdd(EntityType, t =>
				{
					return ClassMap.DeclaredMemberMaps.Select(m => new EntityPropertyMap
					{
						IsKey = m == ClassMap.IdMemberMap,
						ElementName = m.ElementName,
						Property = m.MemberInfo as PropertyInfo
					});
				});
			}
		}
	}

	public class EntityMapper<TEntity> : EntityMapper
	{
		public EntityMapper() : base(typeof(TEntity)) { }
	}
}
