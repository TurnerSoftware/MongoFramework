using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.Mutators
{
	[Flags]
	public enum DbEntityMutatorType
	{
		Select = 1,
		Insert = 2,
		Update = 4
	}
}
