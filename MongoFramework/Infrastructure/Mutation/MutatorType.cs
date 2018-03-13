using System;

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
