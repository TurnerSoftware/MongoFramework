using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Internal;

namespace MongoFramework.Infrastructure.Mapping
{
	public static class EntityMapping
	{
		private static ReaderWriterLockSlim MappingLock { get; } = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
		private static ConcurrentDictionary<Type, IEntityDefinition> EntityDefinitions { get; }
		private static List<IMappingProcessor> MappingProcessors { get; }

		static EntityMapping()
		{
			DriverAbstractionRules.ApplyRules();

			EntityDefinitions = new ConcurrentDictionary<Type, IEntityDefinition>();
			MappingProcessors = new List<IMappingProcessor>();

			AddMappingProcessors(DefaultProcessors.CreateProcessors());
		}

		[Obsolete("This will be removed in a future version")]
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

		[Obsolete("This will be removed in a future version")]
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

		public static void RegisterMapping(Action<MappingBuilder> builder)
		{
			var mappingBuilder = new MappingBuilder(MappingProcessors);
			builder(mappingBuilder);
			RegisterMapping(mappingBuilder);
		}

		public static void RegisterMapping(MappingBuilder mappingBuilder)
		{
			MappingLock.EnterWriteLock();
			try
			{
				foreach (var definition in mappingBuilder.Definitions)
				{
					if (EntityDefinitions.ContainsKey(definition.EntityType))
					{
						continue;
					}

					ApplyMapping(definition);
				}
			}
			finally
			{
				MappingLock.ExitWriteLock();
			}
		}

		private static IEntityDefinition ApplyMapping(EntityDefinitionBuilder definitionBuilder)
		{
			//TODO: Really needs a refactor - easy to see also how messy indexes make the building process
			static string GetElementName(EntityDefinitionBuilder definitionBuilder, PropertyInfo propertyInfo)
			{
				if (propertyInfo.DeclaringType == definitionBuilder.EntityType)
				{
					return definitionBuilder.Properties.First(p => p.PropertyInfo == propertyInfo).ElementName;
				}
				else if (EntityDefinitions.TryGetValue(propertyInfo.DeclaringType, out var definition))
				{
					var property = definition.GetProperty(propertyInfo.Name) ?? throw new ArgumentException($"Property \"{propertyInfo.Name}\" was not found on existing definition for \"{propertyInfo.DeclaringType}\"");
					return property.ElementName;
				}
				else
				{
					var localDefinitionBuilder = definitionBuilder.MappingBuilder.Entity(propertyInfo.DeclaringType);
					return localDefinitionBuilder.Properties.First(p => p.PropertyInfo == propertyInfo).ElementName;
				}
			}

			string EvaluateIndexPath(EntityDefinitionBuilder definitionBuilder, PropertyPath propertyPath)
			{
				var pool = ArrayPool<string>.Shared.Rent(propertyPath.Properties.Count);
				try
				{
					for (var i = 0; i < propertyPath.Properties.Count; i++)
					{
						var propertyInfo = propertyPath.Properties[i];
						pool[i] = GetElementName(definitionBuilder, propertyInfo);
					}
					return string.Join(".", pool, 0, propertyPath.Properties.Count);
				}
				finally
				{
					ArrayPool<string>.Shared.Return(pool);
				}
			}

			IEntityIndexDefinition BuildIndexDefinition(EntityDefinitionBuilder definitionBuilder, EntityIndexBuilder indexBuilder)
			{
				return new EntityIndexDefinition
				{
					IndexPaths = indexBuilder.Properties.Select(p => new EntityIndexPathDefinition
					{
						Path = EvaluateIndexPath(definitionBuilder, p.PropertyPath),
						IndexType = p.IndexType,
						SortOrder = p.SortOrder
					}).ToArray(),
					IndexName = indexBuilder.IndexName,
					IsUnique = indexBuilder.Unique,
					IsTenantExclusive = indexBuilder.TenantExclusive
				};
			}

			var properties = definitionBuilder.Properties.Select(p => new EntityPropertyDefinition
			{
				PropertyInfo = p.PropertyInfo,
				ElementName = p.ElementName
			}).ToArray();

			var definition = new EntityDefinition
			{
				EntityType = definitionBuilder.EntityType,
				CollectionName = definitionBuilder.CollectionName,
				Key = definitionBuilder.KeyBuilder is null ? null : new EntityKeyDefinition
				{
					Property = properties.First(p => p.PropertyInfo == definitionBuilder.KeyBuilder.Property),
					KeyGenerator = definitionBuilder.KeyBuilder.KeyGenerator,
				},
				Properties = properties,
				ExtraElements = definitionBuilder.ExtraElementsProperty is null ? new EntityExtraElementsDefinition
				{
					IgnoreExtraElements = true,
					IgnoreInherited = true
				} : new EntityExtraElementsDefinition
				{
					Property = properties.First(p => p.PropertyInfo == definitionBuilder.ExtraElementsProperty)
				},
				Indexes = definitionBuilder.Indexes.Select(b => BuildIndexDefinition(definitionBuilder, b)).ToArray(),
			};

			if (EntityDefinitions.TryAdd(definition.EntityType, definition))
			{
				DriverMappingInterop.RegisterDefinition(definition);
				return definition;
			}

			throw new InvalidOperationException("Uh oh");
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

					var mappingBuilder = new MappingBuilder(MappingProcessors);
					definition = ApplyMapping(mappingBuilder.Entity(entityType));
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

					var mappingBuilder = new MappingBuilder(MappingProcessors);
					definition = ApplyMapping(mappingBuilder.Entity(entityType));
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
