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
	public static class EntityMapping
	{
		private static ReaderWriterLockSlim MappingLock { get; } = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
		private static ConcurrentDictionary<Type, IEntityDefinition> EntityDefinitions { get; }
		private static List<IMappingProcessor> MappingProcessors { get; }

		static EntityMapping()
		{
			EntityDefinitions = new ConcurrentDictionary<Type, IEntityDefinition>();
			MappingProcessors = new List<IMappingProcessor>();

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
				if (!BsonClassMap.IsClassMapRegistered(entityType))
				{
					MappingLock.EnterWriteLock();

					try
					{
						var classMap = new BsonClassMap(entityType);

						BsonClassMap.RegisterClassMap(classMap);
						classMap.AutoMap();

						foreach (var processor in MappingProcessors)
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
			MappingProcessors.AddRange(mappingProcessors);
		}
		public static void AddMappingProcessor(IMappingProcessor mappingProcessor)
		{
			MappingProcessors.Add(mappingProcessor);
		}
		public static void RemoveMappingProcessor<TProcessor>() where TProcessor : IMappingProcessor
		{
			var matchingItems = MappingProcessors.Where(p => p.GetType() == typeof(TProcessor)).ToArray();
			foreach (var matchingItem in matchingItems)
			{
				MappingProcessors.Remove(matchingItem);
			}
		}
		public static void RemoveAllMappingProcessors()
		{
			MappingProcessors.Clear();
		}
	}
}
