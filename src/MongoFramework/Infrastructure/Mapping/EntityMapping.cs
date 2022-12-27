using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

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

			AddMappingProcessors(DefaultProcessors.CreateProcessors());
		}

		public static IEntityDefinition SetEntityDefinition(IEntityDefinition definition)
		{
			MappingLock.EnterWriteLock();
			try
			{
				return EntityDefinitions.AddOrUpdate(definition.EntityType, definition, (entityType, existingValue) =>
				{
					return definition;
				});
			}
			finally
			{
				MappingLock.ExitWriteLock();
			}
		}

		public static void RemoveEntityDefinition(IEntityDefinition definition)
		{
			EntityDefinitions.TryRemove(definition.EntityType, out _);
		}

		public static void RemoveAllDefinitions()
		{
			EntityDefinitions.Clear();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsValidTypeToMap(Type entityType)
		{
			return entityType.IsClass &&
				entityType != typeof(object) &&
				entityType != typeof(string) &&
				!typeof(BsonValue).IsAssignableFrom(entityType);
		}

		public static bool IsRegistered(Type entityType)
		{
			return EntityDefinitions.ContainsKey(entityType);
		}

		public static IEntityDefinition RegisterType(Type entityType)
		{
			if (!IsValidTypeToMap(entityType))
			{
				throw new ArgumentException("Type is not a valid type to map", nameof(entityType));
			}

			MappingLock.EnterUpgradeableReadLock();
			try
			{
				if (EntityDefinitions.ContainsKey(entityType))
				{
					throw new ArgumentException("Type is already registered", nameof(entityType));
				}

				if (BsonClassMap.IsClassMapRegistered(entityType))
				{
					throw new ArgumentException($"Type is already registered as a {nameof(BsonClassMap)}");
				}

				MappingLock.EnterWriteLock();
				try
				{
					//Now we have the write lock, do one super last minute check
					if (EntityDefinitions.TryGetValue(entityType, out var definition))
					{
						//We will treat success of this check as if we have registered it just now
						return definition;
					}
					definition = new EntityDefinition
					{
						EntityType = entityType
					};

					EntityDefinitions.TryAdd(entityType, definition);

					foreach (var processor in MappingProcessors)
					{
						processor.ApplyMapping(definition, null);
					}

					DriverMappingInterop.RegisterDefinition(definition);
					return definition;
				}
				finally
				{
					MappingLock.ExitWriteLock();
				}
			}
			finally
			{
				MappingLock.ExitUpgradeableReadLock();
			}
		}

		public static IEntityDefinition GetOrCreateDefinition(Type entityType)
		{
			MappingLock.EnterUpgradeableReadLock();
			try
			{
				if (EntityDefinitions.TryGetValue(entityType, out var definition))
				{
					return definition;
				}

				return RegisterType(entityType);
			}
			finally
			{
				MappingLock.ExitUpgradeableReadLock();
			}
		}

		public static bool TryRegisterType(Type entityType, out IEntityDefinition definition)
		{
			if (!IsValidTypeToMap(entityType))
			{
				definition = null;
				return false;
			}

			MappingLock.EnterUpgradeableReadLock();
			try
			{
				if (EntityDefinitions.ContainsKey(entityType) || BsonClassMap.IsClassMapRegistered(entityType))
				{
					definition = null;
					return false;
				}

				MappingLock.EnterWriteLock();
				try
				{
					//Now we have the write lock, do one super last minute check
					if (EntityDefinitions.TryGetValue(entityType, out definition))
					{
						//We will treat success of this check as if we have registered it just now
						return true;
					}

					definition = new EntityDefinition
					{
						EntityType = entityType
					};

					EntityDefinitions.TryAdd(entityType, definition);

					foreach (var processor in MappingProcessors)
					{
						processor.ApplyMapping(definition, null);
					}

					DriverMappingInterop.RegisterDefinition(definition);
					return true;
				}
				finally
				{
					MappingLock.ExitWriteLock();
				}
			}
			finally
			{
				MappingLock.ExitUpgradeableReadLock();
			}
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
