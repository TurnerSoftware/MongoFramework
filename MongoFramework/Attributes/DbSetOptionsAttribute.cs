using System;
using System.Collections.Generic;
using System.Text;
using MongoFramework.Infrastructure;

namespace MongoFramework.Attributes
{
	public abstract class DbSetOptionsAttribute : Attribute
	{
		public abstract IDbSetOptions GetOptions();
	}
}
