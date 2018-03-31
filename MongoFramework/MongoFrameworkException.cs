using System;
using System.Collections.Generic;
using System.Text;

namespace MongoFramework
{
	[Serializable]
	public class MongoFrameworkException : Exception
	{
		public MongoFrameworkException() { }
		public MongoFrameworkException(string message) : base(message) { }
	}
}
