using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Commands;
using MongoFramework.Infrastructure.Indexing;
using MongoFramework.Infrastructure.Linq;
using MongoFramework.Infrastructure.Linq.Processors;
using MongoFramework.Infrastructure.Mutation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MongoFramework
{
	/// <summary>
	/// Basic Mongo "DbSet", providing changeset support and attribute validation
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public class MongoDbSet<TEntity> : IMongoDbSet<TEntity> where TEntity : class
	{
		public IEntityCollection<TEntity> ChangeTracker { get; protected set; }

		protected IMongoDbConnection Connection { get; private set; }
		protected IEntityWriterPipeline<TEntity> EntityWriterPipeline { get; private set; }
		protected IEntityReader<TEntity> EntityReader { get; private set; }
		protected IEntityIndexWriter<TEntity> EntityIndexWriter { get; private set; }

		/// <summary>
		/// Initialise a new entity reader and writer to the specified database.
		/// </summary>
		/// <param name="connection"></param>
		public virtual void SetConnection(IMongoDbConnection connection)
		{
			Connection = connection;
			EntityWriterPipeline = new EntityWriterPipeline<TEntity>(connection);
			EntityReader = new EntityReader<TEntity>(connection);
			EntityIndexWriter = new EntityIndexWriter<TEntity>(connection);
			ChangeTracker = new EntityCollection<TEntity>();

			EntityWriterPipeline.AddCollection(ChangeTracker);
		}

		public virtual TEntity Create()
		{
			var entity = Activator.CreateInstance<TEntity>();
			EntityMutation<TEntity>.MutateEntity(entity, MutatorType.Create, Connection);
			Add(entity);
			return entity;
		}

		/// <summary>
		/// Marks the entity for insertion into the database.
		/// </summary>
		/// <param name="entity"></param>
		public virtual void Add(TEntity entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			ChangeTracker.Update(entity, EntityEntryState.Added);
		}
		/// <summary>
		/// Marks the collection of entities for insertion into the database.
		/// </summary>
		/// <param name="entities"></param>
		public virtual void AddRange(IEnumerable<TEntity> entities)
		{
			if (entities == null)
			{
				throw new ArgumentNullException(nameof(entities));
			}

			foreach (var entity in entities)
			{
				ChangeTracker.Update(entity, EntityEntryState.Added);
			}
		}

		/// <summary>
		/// Marks the entity for updating.
		/// </summary>
		/// <param name="entity"></param>
		public virtual void Update(TEntity entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			ChangeTracker.Update(entity, EntityEntryState.Updated);
		}
		/// <summary>
		/// Marks the collection of entities for updating.
		/// </summary>
		/// <param name="entities"></param>
		public virtual void UpdateRange(IEnumerable<TEntity> entities)
		{
			if (entities == null)
			{
				throw new ArgumentNullException(nameof(entities));
			}

			foreach (var entity in entities)
			{
				ChangeTracker.Update(entity, EntityEntryState.Updated);
			}
		}

		/// <summary>
		/// Marks the entity for deletion.
		/// </summary>
		/// <param name="entity"></param>
		public virtual void Remove(TEntity entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			ChangeTracker.Update(entity, EntityEntryState.Deleted);
		}
		/// <summary>
		/// Marks the collection of entities for deletion.
		/// </summary>
		/// <param name="entities"></param>
		public virtual void RemoveRange(IEnumerable<TEntity> entities)
		{
			if (entities == null)
			{
				throw new ArgumentNullException(nameof(entities));
			}

			foreach (var entity in entities)
			{
				ChangeTracker.Update(entity, EntityEntryState.Deleted);
			}
		}
		/// <summary>
		/// Stages a deletion for a range of entities that match the predicate
		/// </summary>
		/// <param name="targetField"></param>
		public virtual void RemoveRange(Expression<Func<TEntity, bool>> predicate)
		{
			EntityWriterPipeline.StageCommand(new RemoveEntityRangeCommand<TEntity>(predicate));
		}
		/// <summary>
		/// Stages a deletion for the entity that matches the specified ID
		/// </summary>
		/// <param name="entityId"></param>
		public virtual void RemoveById(object entityId)
		{
			EntityWriterPipeline.StageCommand(new RemoveEntityByIdCommand<TEntity>(entityId));
		}

		/// <summary>
		/// Writes all of the items in the changeset to the database.
		/// </summary>
		/// <returns></returns>
		public virtual void SaveChanges()
		{
			EntityIndexWriter.ApplyIndexing();
			EntityWriterPipeline.Write();
		}

		/// <summary>
		/// Writes all of the items in the changeset to the database.
		/// </summary>
		/// <returns></returns>
		public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			await EntityIndexWriter.ApplyIndexingAsync(cancellationToken).ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();
			await EntityWriterPipeline.WriteAsync(cancellationToken).ConfigureAwait(false);
		}

		#region IQueryable Implementation

		private IQueryable<TEntity> GetQueryable()
		{
			var queryable = EntityReader.AsQueryable();
			var provider = queryable.Provider as IMongoFrameworkQueryProvider<TEntity>;
			provider.EntityProcessors.Add(new EntityTrackingProcessor<TEntity>(ChangeTracker));
			return queryable;
		}

		public Expression Expression => GetQueryable().Expression;

		public Type ElementType => GetQueryable().ElementType;

		public IQueryProvider Provider => GetQueryable().Provider;

		public IEnumerator<TEntity> GetEnumerator()
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