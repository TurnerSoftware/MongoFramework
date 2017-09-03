using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure
{
	public interface IDbEntityChangeWriter<TEntity> : IDbEntityWriter<TEntity>
	{
		void Update(DbEntityEntry<TEntity> entry);
		void UpdateRange(IEnumerable<DbEntityEntry<TEntity>> entries);
	}
}
