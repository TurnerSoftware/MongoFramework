using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoFramework.Infrastructure.Commands;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Utilities;

namespace MongoFramework
{
	public class MongoDbBucketSet<TGroup, TSubEntity> : IMongoDbBucketSet<TGroup, TSubEntity>
		where TGroup : class
		where TSubEntity : class
	{
		private IMongoDbContext Context { get; }

		internal int BucketSize { get; }

		internal PropertyDefinition EntityTimeProperty { get; }

		public MongoDbBucketSet(IMongoDbContext context, IDbSetOptions options)
		{
			Check.NotNull(context, nameof(context));
			Context = context;

			if (options is BucketSetOptions bucketOptions)
			{
				if (bucketOptions.BucketSize < 1)
				{
					throw new ArgumentException($"Invalid bucket size of {bucketOptions.BucketSize}");
				}
				BucketSize = bucketOptions.BucketSize;

				var property = EntityMapping.GetOrCreateDefinition(typeof(TSubEntity)).GetProperty(bucketOptions.EntityTimeProperty);
				if (property == null)
				{
					throw new ArgumentException($"Property {bucketOptions.EntityTimeProperty} doesn't exist on bucket item.");
				}

				if (property.PropertyInfo.PropertyType != typeof(DateTime))
				{
					throw new ArgumentException($"Property {bucketOptions.EntityTimeProperty} on bucket item isn't of type DateTime");
				}

				EntityTimeProperty = property;
			}
			else
			{
				throw new ArgumentException("Invalid DbSet options supplied", nameof(options));
			}
		}

		public virtual void Add(TGroup group, TSubEntity entity)
		{
			Check.NotNull(group, nameof(group));
			Check.NotNull(entity, nameof(entity));

			Context.CommandStaging.Add(new AddToBucketCommand<TGroup, TSubEntity>(group, entity, EntityTimeProperty, BucketSize));
		}

		public virtual void AddRange(TGroup group, IEnumerable<TSubEntity> entities)
		{
			Check.NotNull(group, nameof(group));
			Check.NotNull(entities, nameof(entities));

			foreach (var entity in entities)
			{
				Context.CommandStaging.Add(new AddToBucketCommand<TGroup, TSubEntity>(group, entity, EntityTimeProperty, BucketSize));
			}
		}

		public virtual void Remove(TGroup group)
		{
			Check.NotNull(group, nameof(group));

			Context.CommandStaging.Add(new RemoveBucketCommand<TGroup, TSubEntity>(group));
		}

		public virtual IQueryable<TSubEntity> WithGroup(TGroup group)
		{
			return GetQueryable()
				.Where(e => e.Group == group)
				.OrderBy(e => e.Min)
				.SelectMany(e => e.Items);
		}

		public virtual IQueryable<TGroup> Groups()
		{
			return GetQueryable()
				.Select(e => e.Group)
				.Distinct();
		}

		[Obsolete("Use SaveChanges on the IMongoDbContext")]
		public void SaveChanges()
		{
			Context.SaveChanges();
		}

		[Obsolete("Use SaveChangesAsync on the IMongoDbContext")]
		public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			await Context.SaveChangesAsync(cancellationToken);
		}

		#region IQueryable Implementation

		private IQueryable<EntityBucket<TGroup, TSubEntity>> GetQueryable()
		{
			return Context.Query<EntityBucket<TGroup, TSubEntity>>();
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
