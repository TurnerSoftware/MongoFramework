using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure
{
	public interface IDbEntityReader<TEntity>
	{
		IDbEntityMapper<TEntity> EntityMapper { get; }
		IQueryable<TEntity> AsQueryable();
	}
}
