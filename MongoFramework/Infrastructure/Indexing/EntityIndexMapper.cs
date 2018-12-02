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

		public EntityIndexMapper(Type entityType)
		{
			EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
			EntityMapper = new EntityMapper(entityType);
		}

		public EntityIndexMapper(IEntityMapper entityMapper)
		{
			EntityMapper = entityMapper ?? throw new ArgumentNullException(nameof(entityMapper));
			EntityType = entityMapper.EntityType;
		}

		public string GetCollectionName()
		{
			return EntityMapper.GetCollectionName();
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

	public class EntityIndexMapper<TEntity> : EntityIndexMapper
	{
		public EntityIndexMapper() : base(typeof(TEntity)) { }
		public EntityIndexMapper(IEntityMapper entityMapper) : base(entityMapper)
		{
			if (typeof(TEntity) != entityMapper.EntityType)
			{
				throw new InvalidOperationException("Generic type does not match IDbEntityMapper's EntityType");
			}
		}
	}
}
