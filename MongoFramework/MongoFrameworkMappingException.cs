using System;
using System.Collections.Generic;
using System.Text;

namespace MongoFramework
{
	[Serializable]
	public class MongoFrameworkMappingException : MongoFrameworkException
	{
		public MongoFrameworkMappingException() { }
		public MongoFrameworkMappingException(string message) : base(message) { }
	}
}
