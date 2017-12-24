using MongoFramework.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.Mutation
{
	public interface IEntityMutator<TEntity>
	{
		void MutateEntity(TEntity entity, MutatorType mutationType, IEntityMapper entityMapper);
	}
}
