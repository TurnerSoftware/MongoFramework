using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace MongoFramework.Infrastructure.Mapping;

public static partial class EntityMapping
{
	private static ReaderWriterLockSlim MappingLock { get; } = new(LockRecursionPolicy.SupportsRecursion);
	private static readonly ConcurrentDictionary<Type, EntityDefinition> EntityDefinitions = new();

	static EntityMapping()
	{
		DriverAbstractionRules.ApplyRules();
		AddMappingProcessors(DefaultMappingProcessors.Processors);
	}

	internal static void RemoveAllDefinitions()
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

	public static EntityDefinition GetOrCreateDefinition(Type entityType)
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

	public static bool TryRegisterType(Type entityType, out EntityDefinition definition)
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
				mappingBuilder.Entity(entityType);

				RegisterMapping(mappingBuilder);

				definition = EntityDefinitions[entityType];
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

	public static EntityDefinition RegisterType(Type entityType)
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
				mappingBuilder.Entity(entityType);

				RegisterMapping(mappingBuilder);

				return EntityDefinitions[entityType];
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
}
