using MongoDB.Bson.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace MongoFramework.Infrastructure.Mapping
{
	public class EntityMapping
	{
		private static ReaderWriterLockSlim MappingLock { get; } = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
		private static ConcurrentDictionary<Type, IEntityDefinition> EntityDefinitions { get; }
		private static ConcurrentDictionary<Type, IMappingProcessor> MappingProcessors { get; }

		static EntityMapping()
		{
			EntityDefinitions = new ConcurrentDictionary<Type, IEntityDefinition>();
			MappingProcessors = new ConcurrentDictionary<Type, IMappingProcessor>();

			AddMappingProcessors(DefaultMappingPack.Instance.Processors);
		}
		public static IEntityDefinition SetEntityDefinition(IEntityDefinition definition)
		{
			return EntityDefinitions.AddOrUpdate(definition.EntityType, definition, (entityType, existingValue) =>
			{
				return definition;
			});
		}

		public static void RemoveEntityDefinition(IEntityDefinition definition)
		{
			EntityDefinitions.TryRemove(definition.EntityType, out _);
		}

		public static void RemoveAllDefinitions()
		{
			EntityDefinitions.Clear();
		}

		public static bool IsRegistered(Type entityType)
		{
			return EntityDefinitions.ContainsKey(entityType);
		}

		public static IEntityDefinition RegisterType(Type entityType)
		{
			if (IsRegistered(entityType))
			{
				throw new ArgumentException("Type is already registered", nameof(entityType));
			}

			var definition = new EntityDefinition
			{
				EntityType = entityType
			};

			MappingLock.EnterUpgradeableReadLock();
			try
			{
				//For reasons unknown to me, you can't just call "BsonClassMap.LookupClassMap" as that "freezes" the class map
				//Instead, you must do the lookup and initial creation yourself.
				var classMap = BsonClassMap.GetRegisteredClassMaps()
					.Where(cm => cm.ClassType == entityType).FirstOrDefault();

				if (classMap == null)
				{
					MappingLock.EnterWriteLock();

					try
					{
						classMap = new BsonClassMap(entityType);

						BsonClassMap.RegisterClassMap(classMap);
						classMap.AutoMap();

						foreach (var processor in MappingProcessors.Values)
						{
							processor.ApplyMapping(definition, classMap);
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

			return SetEntityDefinition(definition);
		}

		public static IEntityDefinition GetOrCreateDefinition(Type entityType)
		{
			return EntityDefinitions.GetOrAdd(entityType, t =>
			{
				return RegisterType(entityType);
			});
		}

		public static void AddMappingProcessors(IEnumerable<IMappingProcessor> mappingProcessors)
		{
			foreach (var processor in mappingProcessors)
			{
				MappingProcessors.TryAdd(processor.GetType(), processor);
			}
		}
		public static void AddMappingProcessor<TProcessor>(TProcessor mappingProcessor) where TProcessor : IMappingProcessor
		{
			MappingProcessors.TryAdd(typeof(TProcessor), mappingProcessor);
		}
		public static void RemoveMappingProcessor<TProcessor>() where TProcessor : IMappingProcessor
		{
			MappingProcessors.TryRemove(typeof(TProcessor), out _);
		}
		public static void RemoveAllMappingProcessors()
		{
			MappingProcessors.Clear();
		}
	}
}
