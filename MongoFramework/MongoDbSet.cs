﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Indexing;
using MongoFramework.Infrastructure.Linq;
using MongoFramework.Infrastructure.Linq.Processors;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework
{
	/// <summary>
	/// Basic Mongo "DbSet", providing changeset support and attribute validation
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public class MongoDbSet<TEntity> : IMongoDbSet<TEntity>, IAsyncMongoDbSet
	{
		public IDbChangeTracker<TEntity> ChangeTracker { get; private set; } = new DbChangeTracker<TEntity>();

		private IAsyncDbEntityChangeWriter<TEntity> EntityWriter { get; set; }
		private IDbEntityReader<TEntity> EntityReader { get; set; }
		private IEntityIndexWriter<TEntity> EntityIndexWriter { get; set; }

		/// <summary>
		/// Whether any entity validation is performed prior to saving changes. (Default is true)
		/// </summary>
		public bool PerformEntityValidation { get; set; } = true;

		public MongoDbSet() { }

		/// <summary>
		/// Creates a new MongoDbSet using the connection string in the configuration that matches the specified connection name.
		/// </summary>
		/// <param name="connectionName">The name of the connection string stored in the configuration.</param>
		public MongoDbSet(string connectionName)
		{
			var mongoUrl = MongoDbUtility.GetMongoUrlFromConfig(connectionName);

			if (mongoUrl == null)
			{
				throw new MongoConfigurationException("No connection string found with the name \'" + connectionName + "\'");
			}

			SetDatabase(MongoDbUtility.GetDatabase(mongoUrl));
		}

		/// <summary>
		/// Creates a new MongoDbSet using the specified connection string and database combination.
		/// </summary>
		/// <param name="connectionString">The connection string to the server</param>
		/// <param name="databaseName">The database name on the server</param> 
		public MongoDbSet(string connectionString, string databaseName)
		{
			SetDatabase(MongoDbUtility.GetDatabase(connectionString, databaseName));
		}

		/// <summary>
		/// Initialise a new entity reader and writer to the specified database.
		/// </summary>
		/// <param name="database"></param>
		public void SetDatabase(IMongoDatabase database)
		{
			var entityMapper = new EntityMapper<TEntity>();
			EntityWriter = new AsyncDbEntityWriter<TEntity>(database, entityMapper);
			EntityReader = new DbEntityReader<TEntity>(database, entityMapper);

			//TODO: Look at this again in the future, this seems unnecessarily complex
			var indexMapper = new EntityIndexMapper<TEntity>(entityMapper);
			var collection = database.GetCollection<TEntity>(entityMapper.GetCollectionName());
			EntityIndexWriter = new EntityIndexWriter<TEntity>(collection, indexMapper);
		}

		/// <summary>
		/// Marks the entity for insertion into the database.
		/// </summary>
		/// <param name="entity"></param>
		public virtual void Add(TEntity entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			ChangeTracker.Update(entity, DbEntityEntryState.Added);
		}
		/// <summary>
		/// Marks the collection of entities for insertion into the database.
		/// </summary>
		/// <param name="entities"></param>
		public virtual void AddRange(IEnumerable<TEntity> entities)
		{
			if (entities == null)
			{
				throw new ArgumentNullException("entities");
			}

			ChangeTracker.UpdateRange(entities, DbEntityEntryState.Added);
		}

		/// <summary>
		/// Marks the entity for updating.
		/// </summary>
		/// <param name="entity"></param>
		public virtual void Update(TEntity entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			ChangeTracker.Update(entity, DbEntityEntryState.Updated);
		}
		/// <summary>
		/// Marks the collection of entities for updating.
		/// </summary>
		/// <param name="entities"></param>
		public virtual void UpdateRange(IEnumerable<TEntity> entities)
		{
			if (entities == null)
			{
				throw new ArgumentNullException("entities");
			}

			ChangeTracker.UpdateRange(entities, DbEntityEntryState.Updated);
		}

		/// <summary>
		/// Marks the entity for deletion.
		/// </summary>
		/// <param name="entity"></param>
		public virtual void Remove(TEntity entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			ChangeTracker.Update(entity, DbEntityEntryState.Deleted);
		}
		/// <summary>
		/// Marks the collection of entities for deletion.
		/// </summary>
		/// <param name="entities"></param>
		public virtual void RemoveRange(IEnumerable<TEntity> entities)
		{
			if (entities == null)
			{
				throw new ArgumentNullException("entities");
			}

			ChangeTracker.UpdateRange(entities, DbEntityEntryState.Deleted);
		}

		private void CheckEntityValidation()
		{
			if (PerformEntityValidation)
			{
				var savingEntities = ChangeTracker.GetEntries()
					.Where(e => e.State == DbEntityEntryState.Added || e.State == DbEntityEntryState.Updated)
					.Select(e => e.Entity);

				foreach (var savingEntity in savingEntities)
				{
					var validationContext = new ValidationContext(savingEntity);
					Validator.ValidateObject(savingEntity, validationContext);
				}
			}
		}

		/// <summary>
		/// Writes all of the items in the changeset to the database.
		/// </summary>
		/// <returns></returns>
		public virtual void SaveChanges()
		{
			EntityIndexWriter.ApplyIndexing();
			ChangeTracker.DetectChanges();
			CheckEntityValidation();
			EntityWriter.WriteChanges(ChangeTracker);
		}

		/// <summary>
		/// Writes all of the items in the changeset to the database.
		/// </summary>
		/// <returns></returns>
		public async Task SaveChangesAsync()
		{
			await EntityIndexWriter.ApplyIndexingAsync();
			ChangeTracker.DetectChanges();
			CheckEntityValidation();
			await EntityWriter.WriteChangesAsync(ChangeTracker);
		}

		#region IQueryable Implementation

		private IQueryable<TEntity> GetQueryable()
		{
			if (EntityReader == null)
			{
				throw new InvalidOperationException("No IDbEntityReader has been set.");
			}

			var queryable = EntityReader.AsQueryable() as IMongoFrameworkQueryable<TEntity, TEntity>;
			queryable.EntityProcessors.Add(new EntityTrackingProcessor<TEntity>(ChangeTracker));
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