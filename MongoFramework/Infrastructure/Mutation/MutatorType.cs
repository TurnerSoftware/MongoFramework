using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.Mutation
{
	[Flags]
	public enum MutatorType
	{
		Select = 1,
		Insert = 2,
		Update = 4
	}
}
