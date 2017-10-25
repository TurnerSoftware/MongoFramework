using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.Mutators
{
	public interface IDbEntityMutator<TEntity>
	{
		void MutateEntity(TEntity entity, DbEntityMutatorType mutationType, IDbEntityMapper entityMapper);
	}
}
