using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using System.Linq.Expressions;
using MongoFramework.Core;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace MongoFramework
{
	/// <summary>
	/// Basic Mongo "DbSet", providing changeset support and attribute validation
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public class MongoDbSet<TEntity> : IMongoDbSet<TEntity>
	{
		public EntityChangeSet<TEntity> ChangeSet { get; private set; } = new EntityChangeSet<TEntity>();

		private IDbEntityWriter<TEntity> dbWriter { get; set; }
		private IDbEntityReader<TEntity> dbReader { get; set; }

		/// <summary>
		/// Whether any entity validation is performed prior to saving changes. (Default is true)
		/// </summary>
		public bool PerformEntityValidation { get; set; } = true;

		public MongoDbSet() { }

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
		
		/// <param name="connectionString">The connection string to the server</param>
		/// <param name="databaseName">The database name on the server</param>
		public MongoDbSet(string connectionString, string databaseName)
		{
			SetDatabase(MongoDbUtility.GetDatabase(connectionString, databaseName));
		}

		public static MongoDbSet<TEntity> CreateWithDatabase(IMongoDatabase database)
		{
			var dbSet = new MongoDbSet<TEntity>();
			dbSet.SetDatabase(database);
			return dbSet;
		}

		public void SetDatabase(IMongoDatabase database)
		{
			dbWriter = new DbEntityWriter<TEntity>(database);
			dbReader = new DbEntityReader<TEntity>(database);
		}

		public void SetDatabase(IMongoDatabase database, string collectionName)
		{
			dbWriter = new DbEntityWriter<TEntity>(database, collectionName);
			dbReader = new DbEntityReader<TEntity>(database, collectionName);
		}

		/// <summary>
		/// Marks the entity for insertion into the database.
		/// </summary>
		/// <param name="entity"></param>
		public void Add(TEntity entity)
		{
			ChangeSet.SetEntityState(entity, EntityState.Added);
		}
		/// <summary>
		/// Marks the collection of entities for insertion into the database.
		/// </summary>
		/// <param name="entities"></param>
		public void AddRange(IEnumerable<TEntity> entities)
		{
			foreach (var entity in entities)
			{
				Add(entity);
			}
		}

		/// <summary>
		/// Marks the entity for updating.
		/// </summary>
		/// <param name="entity"></param>
		public void Update(TEntity entity)
		{
			ChangeSet.SetEntityState(entity, EntityState.Updated);
		}
		/// <summary>
		/// Marks the collection of entities for updating.
		/// </summary>
		/// <param name="entities"></param>
		public void UpdateRange(IEnumerable<TEntity> entities)
		{
			foreach (var entity in entities)
			{
				Update(entity);
			}
		}

		/// <summary>
		/// Marks the entity for deletion.
		/// </summary>
		/// <param name="entity"></param>
		public void Delete(TEntity entity)
		{
			ChangeSet.SetEntityState(entity, EntityState.Deleted);
		}
		/// <summary>
		/// Marks the collection of entities for deletion.
		/// </summary>
		/// <param name="entities"></param>
		public void DeleteRange(IEnumerable<TEntity> entities)
		{
			foreach (var entity in entities)
			{
				Delete(entity);
			}
		}
		/// <summary>
		/// Deletes the specified the entity matching the Id. This deletion will be performed immediately.
		/// </summary>
		/// <param name="id"></param>
		public void DeleteById(object id)
		{
			dbWriter.DeleteById(id);
		}

		/// <summary>
		/// Writes all of the items in the changeset to the database.
		/// </summary>
		public void SaveChanges()
		{
			if (dbWriter == null)
			{
				throw new InvalidOperationException("No database has been specified.");
			}

			var insertingEntities = ChangeSet.GetEntitiesByEntityState(EntityState.Added);
			var updatingEntities = ChangeSet.GetEntitiesByEntityState(EntityState.Updated);
			var deletingEntities = ChangeSet.GetEntitiesByEntityState(EntityState.Deleted);

			if (PerformEntityValidation)
			{
				var savingEntities = insertingEntities.Concat(updatingEntities);
				foreach (var savingEntity in savingEntities)
				{
					var validationContext = new ValidationContext(savingEntity);
					Validator.ValidateObject(savingEntity, validationContext);
				}
			}

			if (insertingEntities.Count() > 0)
			{
				dbWriter.InsertEntities(insertingEntities);
			}

			if (updatingEntities.Count() > 0)
			{
				dbWriter.UpdateEntities(updatingEntities);
			}

			if (deletingEntities.Count() > 0)
			{
				dbWriter.DeleteEntities(deletingEntities);
			}

			ChangeSet.ClearChanges();
		}

		#region IQueryable Implementation

		public Expression Expression
		{
			get
			{
				return dbReader.AsQueryable().Expression;
			}
		}

		public Type ElementType
		{
			get
			{
				return dbReader.AsQueryable().ElementType;
			}
		}

		public IQueryProvider Provider
		{
			get
			{
				return dbReader.AsQueryable().Provider;
			}
		}

		public IEnumerator<TEntity> GetEnumerator()
		{
			return dbReader.AsQueryable().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return dbReader.AsQueryable().GetEnumerator();
		}

		#endregion
	}
}