﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
		public static IEntityDefinition SetEntityDefinition(IEntityDefinition definition) => EntityDefinitions.AddOrUpdate(definition.EntityType, definition, (entityType, existingValue) => definition);
		public static void RemoveEntityDefinition(IEntityDefinition definition) => EntityDefinitions.TryRemove(definition.EntityType, out _);
		public static void RemoveAllDefinitions() => EntityDefinitions.Clear();
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsValidTypeToMap(Type entityType)
		{
			return entityType.IsClass &&
				entityType != typeof(object) &&
				entityType != typeof(string) &&
				!typeof(BsonValue).IsAssignableFrom(entityType);
		}
		public static bool IsRegistered(Type entityType) => EntityDefinitions.ContainsKey(entityType);
		public static IEntityDefinition RegisterType(Type entityType)
		{
			if (!IsValidTypeToMap(entityType))
			{
				throw new ArgumentException("Type is not a valid type to map", nameof(entityType));
			}

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
		public static IEntityDefinition GetOrCreateDefinition(Type entityType) => EntityDefinitions.GetOrAdd(entityType, t => RegisterType(entityType));
		public static void AddMappingProcessors(IEnumerable<IMappingProcessor> mappingProcessors) => MappingProcessors.AddRange(mappingProcessors);
		public static void AddMappingProcessor(IMappingProcessor mappingProcessor) => MappingProcessors.Add(mappingProcessor);
		public static void RemoveMappingProcessor<TProcessor>() where TProcessor : IMappingProcessor
		{
			var matchingItems = MappingProcessors.Where(p => p.GetType() == typeof(TProcessor)).ToArray();
			foreach (var matchingItem in matchingItems)
			{
				MappingProcessors.Remove(matchingItem);
			}
		}
		public static void RemoveAllMappingProcessors() => MappingProcessors.Clear();
	}
}
