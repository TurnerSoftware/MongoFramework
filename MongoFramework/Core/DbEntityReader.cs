using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Core
{
	public class DbEntityReader<TEntity> : IDbEntityReader<TEntity>
	{
		public IMongoDatabase Database { get; set; }
		public string CollectionName { get; private set; }

		private IDbEntityWorkflow<TEntity> entityWorkflow { get; set; }

		public DbEntityReader(IMongoDatabase database) : this(database, null) { }

		public DbEntityReader(IMongoDatabase database, string collectionName)
		{
			Database = database;

			entityWorkflow = new DbEntityWorkflow<TEntity>();
			entityWorkflow.ConfigureMappedFields();

			if (!string.IsNullOrEmpty(collectionName))
			{
				CollectionName = collectionName;
			}
			else
			{
				CollectionName = entityWorkflow.GetCollectionName();
			}
		}
		
		private IMongoCollection<TEntity> GetCollection()
		{
			return Database.GetCollection<TEntity>(CollectionName);
		}

		public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> criteria)
		{
			return GetCollection().Find(criteria).ToList();
		}

		public IQueryable<TEntity> AsQueryable()
		{
			return GetCollection().AsQueryable();
		}
	}
}
