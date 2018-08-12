using MongoFramework.Infrastructure.Mapping;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MongoFramework.Infrastructure.Indexing
{
	public class EntityIndexMapper : IEntityIndexMapper
	{
		public Type EntityType { get; private set; }
		private IEntityMapper EntityMapper { get; set; }

		private static ConcurrentDictionary<Type, IEnumerable<IEntityIndexMap>> EntityIndexCache { get; set; }

		static EntityIndexMapper()
		{
			EntityIndexCache = new ConcurrentDictionary<Type, IEnumerable<IEntityIndexMap>>();
		}

		public EntityIndexMapper(IEntityMapper entityMapper)
		{
			EntityMapper = entityMapper ?? throw new ArgumentNullException(nameof(entityMapper));
			EntityType = entityMapper.EntityType;
		}

		public IEnumerable<IEntityIndexMap> GetIndexMapping()
		{
			return EntityIndexCache.GetOrAdd(EntityType, t =>
			{
				return EntityMapper.TraverseMapping().SelectMany(m =>
					m.Property.GetCustomAttributes(typeof(IEntityIndex), false).Select(a => new EntityIndexMap
					{
						ElementName = m.ElementName,
						FullPath = m.FullPath,
						Index = a as IEntityIndex
					})
				);
			});
		}
	}
}
