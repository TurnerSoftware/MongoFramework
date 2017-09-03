using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.Mutators
{
	public interface IDbEntityMutator<TEntity>
	{
		DbEntityMutatorType MutatorType { get; }
		void MutateEntity(TEntity entity, IDbEntityMapper<TEntity> descriptor);
	}
}
