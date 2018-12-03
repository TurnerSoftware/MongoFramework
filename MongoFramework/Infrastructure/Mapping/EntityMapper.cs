using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace MongoFramework.Infrastructure.Mapping
{
	public class EntityMapper : IEntityMapper
	{
		public Type EntityType { get; private set; }
		private BsonClassMap ClassMap { get; set; }
		private IMongoDbConnection Connection { get; }

		private static ReaderWriterLockSlim MappingLock { get; } = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
		private static ConcurrentDictionary<Type, IEnumerable<IEntityPropertyMap>> EntityMapCache { get; set; }

		static EntityMapper()
		{
			EntityMapCache = new ConcurrentDictionary<Type, IEnumerable<IEntityPropertyMap>>();
		}

		public EntityMapper(Type entityType, IMongoDbConnection connection)
		{
			EntityType = entityType ?? throw new ArgumentNullException("entityType");
			Connection = connection;
			InitialiseClassMap();
		}

		private void InitialiseClassMap()
		{
			MappingLock.EnterUpgradeableReadLock();
			try
			{
				//For reasons unknown to me, you can't just call "BsonClassMap.LookupClassMap" as that "freezes" the class map
				//Instead, you must do the lookup and initial creation yourself.
				var classMaps = BsonClassMap.GetRegisteredClassMaps();
				ClassMap = classMaps.Where(cm => cm.ClassType == EntityType).FirstOrDefault();

				if (ClassMap == null)
				{
					MappingLock.EnterWriteLock();
					try
					{
						ClassMap = new BsonClassMap(EntityType);
						ClassMap.AutoMap();
						BsonClassMap.RegisterClassMap(ClassMap);

						foreach (var processor in DefaultMappingPack.Instance.Processors)
						{
							processor.ApplyMapping(EntityType, ClassMap, Connection);
						}
					}
					finally
					{
						MappingLock.ExitWriteLock();
					}
				}
			}
			finally
			{
				MappingLock.ExitUpgradeableReadLock();
			}
		}

		public string GetCollectionName()
		{
			var tableAttribute = EntityType.GetCustomAttribute<TableAttribute>();

			if (tableAttribute == null && EntityType.IsGenericType && EntityType.GetGenericTypeDefinition() == typeof(EntityBucket<,>))
			{
				var groupProperty = EntityType.GetProperty("Group", BindingFlags.Public | BindingFlags.Instance);
				tableAttribute = groupProperty.GetCustomAttribute<TableAttribute>();
				if (tableAttribute == null)
				{
					return groupProperty.PropertyType.Name;
				}
			}

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

		public object GetDefaultId()
		{
			var idPropertyType = GetEntityMapping().Where(m => m.IsKey).Select(m => m.PropertyType).FirstOrDefault();
			if (idPropertyType.IsValueType)
			{
				return Activator.CreateInstance(idPropertyType);
			}
			return null;
		}

		public IEnumerable<IEntityPropertyMap> GetEntityMapping()
		{
			return GetEntityMapping(true);
		}

		private IEnumerable<IEntityPropertyMap> GetEntityMapping(bool includeInherited)
		{
			if (includeInherited && EntityType.BaseType != typeof(object))
			{
				var declaredProperties = GetEntityMapping(false);
				var inheritedProperties = new EntityMapper(EntityType.BaseType, Connection).GetEntityMapping(true);
				return declaredProperties.Concat(inheritedProperties);
			}
			else
			{
				return EntityMapCache.GetOrAdd(EntityType, t =>
				{
					return ClassMap.DeclaredMemberMaps.Select(m => new EntityPropertyMap
					{
						EntityType = t,
						IsKey = m == ClassMap.IdMemberMap,
						ElementName = m.ElementName,
						FullPath = m.ElementName,
						PropertyType = (m.MemberInfo as PropertyInfo).PropertyType,
						Property = m.MemberInfo as PropertyInfo
					});
				});
			}
		}

		public IEnumerable<IEntityPropertyMap> TraverseMapping()
		{
			var stack = new Stack<TraversalState>();
			stack.Push(new TraversalState
			{
				TypeHierarchy = new HashSet<Type> { EntityType },
				CurrentMap = GetEntityMapping()
			});

			while (stack.Count > 0)
			{
				var state = stack.Pop();
				foreach (var map in state.CurrentMap)
				{
					yield return map;

					if (map.PropertyType.IsClass && !state.TypeHierarchy.Contains(map.PropertyType))
					{
						var nestedMapping = new EntityMapper(map.PropertyType, Connection)
							.GetEntityMapping()
							.Select(m => new EntityPropertyMap
							{
								EntityType = m.EntityType,
								IsKey = m.IsKey,
								ElementName = m.ElementName,
								FullPath = $"{map.FullPath}.{m.ElementName}",
								PropertyType = m.PropertyType,
								Property = m.Property
							});

						stack.Push(new TraversalState
						{
							TypeHierarchy = new HashSet<Type>(state.TypeHierarchy)
							{
								map.PropertyType
							},
							CurrentMap = nestedMapping
						});
					}
				}
			}
		}

		private class TraversalState
		{
			public HashSet<Type> TypeHierarchy { get; set; }
			public IEnumerable<IEntityPropertyMap> CurrentMap { get; set; }
		}
	}

	public class EntityMapper<TEntity> : EntityMapper where TEntity : class
	{
		public EntityMapper(IMongoDbConnection connection) : base(typeof(TEntity), connection) { }
	}
}
