using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Indexing;
using MongoFramework.Infrastructure.Linq;
using MongoFramework.Infrastructure.Linq.Processors;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework
{
	public class MongoDbBucketSet<TGroup, TSubEntity> : IMongoDbBucketSet<TGroup, TSubEntity> where TGroup : class
	{
		private IEntityWriter<EntityBucket<TGroup, TSubEntity>> EntityWriter { get; set; }
		private IEntityReader<EntityBucket<TGroup, TSubEntity>> EntityReader { get; set; }
		private IEntityIndexWriter<EntityBucket<TGroup, TSubEntity>> EntityIndexWriter { get; set; }

		private EntityBucketCollection<TGroup, TSubEntity> BucketCollection { get; set; }
		private IEntityChangeTracker<EntityBucket<TGroup, TSubEntity>> ChangeTracker { get; } = new EntityChangeTracker<EntityBucket<TGroup, TSubEntity>>();

		public int BucketSize { get; }

		public MongoDbBucketSet(IDbSetOptions options)
		{
			if (options is BucketSetOptions bucketOptions)
			{
				if (bucketOptions.BucketSize < 1)
				{
					throw new ArgumentException($"Invalid bucket size of {bucketOptions.BucketSize}");
				}
				BucketSize = bucketOptions.BucketSize;
			}
			else
			{
				throw new ArgumentException("Invalid DbSet options supplied", nameof(options));
			}
		}

		public void SetDatabase(IMongoDatabase database)
		{
			var entityMapper = new EntityMapper<EntityBucket<TGroup, TSubEntity>>();
			EntityWriter = new EntityWriter<EntityBucket<TGroup, TSubEntity>>(database, entityMapper);
			EntityReader = new EntityReader<EntityBucket<TGroup, TSubEntity>>(database, entityMapper);
			
			//TODO: Look at this again in the future, this seems unnecessarily complex 
			var indexMapper = new EntityIndexMapper<EntityBucket<TGroup, TSubEntity>>(entityMapper);
			var collection = database.GetCollection<EntityBucket<TGroup, TSubEntity>>(entityMapper.GetCollectionName());
			EntityIndexWriter = new EntityIndexWriter<EntityBucket<TGroup, TSubEntity>>(collection, indexMapper);

			BucketCollection = new EntityBucketCollection<TGroup, TSubEntity>(EntityReader, BucketSize);
			ChangeTracker.Clear();
		}

		public virtual void Add(TGroup group, TSubEntity entity)
		{
			if (group == null)
			{
				throw new ArgumentNullException(nameof(group));
			}

			BucketCollection.AddEntity(group, entity);
		}

		public virtual void AddRange(TGroup group, IEnumerable<TSubEntity> entities)
		{
			if (group == null)
			{
				throw new ArgumentNullException(nameof(group));
			}

			if (entities == null)
			{
				throw new ArgumentNullException(nameof(entities));
			}

			foreach (var entity in entities)
			{
				BucketCollection.AddEntity(group, entity);
			}
		}

		public virtual IQueryable<TSubEntity> WithGroup(TGroup group)
		{
			var totalItemCount = EntityReader.AsQueryable().Where(e => e.Group == group).Sum(e => e.ItemCount);
			return EntityReader.AsQueryable().Where(e => e.Group == group).OrderBy(e => e.Index).SelectMany(e => e.Items).Take(totalItemCount);
		}

		public virtual IQueryable<TGroup> Groups()
		{
			return EntityReader.AsQueryable().Select(e => e.Group).Distinct();
		}

		public virtual void SaveChanges()
		{
			EntityIndexWriter.ApplyIndexing();
			var entityCollection = BucketCollection.AsEntityCollection();
			EntityWriter.Write(entityCollection);
			EntityWriter.Write(ChangeTracker);
			BucketCollection.Clear();
			ChangeTracker.CommitChanges();
		}

		public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			await EntityIndexWriter.ApplyIndexingAsync();
			cancellationToken.ThrowIfCancellationRequested();
			var entityCollection = BucketCollection.AsEntityCollection();
			await EntityWriter.WriteAsync(entityCollection);
			await EntityWriter.WriteAsync(ChangeTracker);
			BucketCollection.Clear();
			ChangeTracker.CommitChanges();
		}

		#region IQueryable Implementation

		private IQueryable<EntityBucket<TGroup, TSubEntity>> GetQueryable()
		{
			var queryable = EntityReader.AsQueryable() as IMongoFrameworkQueryable<EntityBucket<TGroup, TSubEntity>, EntityBucket<TGroup, TSubEntity>>;
			queryable.EntityProcessors.Add(new EntityTrackingProcessor<EntityBucket<TGroup, TSubEntity>>(ChangeTracker));
			return queryable;
		}
	
		public Type ElementType => GetQueryable().ElementType;

		public Expression Expression => GetQueryable().Expression;

		public IQueryProvider Provider => GetQueryable().Provider;

		public IEnumerator<EntityBucket<TGroup, TSubEntity>> GetEnumerator()
		{
			return GetQueryable().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
