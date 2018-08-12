using System;
using System.Collections.Generic;

namespace MongoFramework.Internal
{
	public interface IDbSetFinder
	{
		IEnumerable<DbSetInfo> FindSets(Type contextType);
	}
}