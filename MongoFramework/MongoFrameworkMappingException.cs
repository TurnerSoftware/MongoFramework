using System;

namespace MongoFramework
{
	[Serializable]
	public class MongoFrameworkMappingException : MongoFrameworkException
	{
		public MongoFrameworkMappingException() { }
		public MongoFrameworkMappingException(string message) : base(message) { }
	}
}
