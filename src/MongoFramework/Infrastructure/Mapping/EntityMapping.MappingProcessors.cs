using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace MongoFramework.Infrastructure.Mapping;

public static partial class EntityMapping
{
	private static readonly List<IMappingProcessor> MappingProcessors = new();

	private static void WithMappingWriteLock(Action action)
	{
		MappingLock.EnterWriteLock();
		try
		{
			action();
		}
		finally
		{
			MappingLock.ExitWriteLock();
		}
	}

	public static void AddMappingProcessor(IMappingProcessor mappingProcessor)
	{
		WithMappingWriteLock(() => MappingProcessors.Add(mappingProcessor));
	}

	public static void AddMappingProcessors(IEnumerable<IMappingProcessor> mappingProcessors)
	{
		WithMappingWriteLock(() => MappingProcessors.AddRange(mappingProcessors));
	}

	public static void RemoveMappingProcessor<TProcessor>() where TProcessor : IMappingProcessor
	{
		WithMappingWriteLock(() =>
		{
			var matchingItems = MappingProcessors.Where(p => p.GetType() == typeof(TProcessor)).ToArray();
			foreach (var matchingItem in matchingItems)
			{
				MappingProcessors.Remove(matchingItem);
			}
		});
	}

	public static void RemoveAllMappingProcessors()
	{
		WithMappingWriteLock(MappingProcessors.Clear);
	}
}
