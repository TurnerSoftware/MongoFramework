using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Core
{
	public interface IDbEntityWriter<TEntity>
	{
		void InsertEntity(TEntity entity);
		void InsertEntities(IEnumerable<TEntity> entities);
		void UpdateEntity(TEntity entity);
		void UpdateEntities(IEnumerable<TEntity> entities);
		void DeleteEntity(TEntity entity);
		void DeleteEntities(IEnumerable<TEntity> entities);
		void DeleteMatching(Expression<Func<TEntity, bool>> criteria);
		void DeleteById(object id);
	}
}
