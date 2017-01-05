using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Core
{
	public interface IDbEntityReader<TEntity>
	{
		IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> criteria);
		IQueryable<TEntity> AsQueryable();
	}
}
