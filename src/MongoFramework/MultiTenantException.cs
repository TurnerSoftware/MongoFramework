using System;
using System.Collections.Generic;
using System.Text;

namespace MongoFramework
{
	public class MultiTenantException : Exception
	{
		public MultiTenantException(string message, Exception innerException = null)
			: base(message, innerException) { }
	}
}
