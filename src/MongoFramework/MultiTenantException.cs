using System;

namespace MongoFramework
{
	public class MultiTenantException : Exception
	{
		public MultiTenantException(string message, Exception innerException = null)
			: base(message, innerException) { }
	}
}
