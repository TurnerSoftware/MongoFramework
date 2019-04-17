using System;

namespace MongoFramework.Attributes
{
	public abstract class DbSetOptionsAttribute : Attribute
	{
		public abstract IDbSetOptions GetOptions();
	}
}
