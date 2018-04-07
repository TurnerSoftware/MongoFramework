using System;

namespace MongoFramework
{
	[Serializable]
	public class MongoFrameworkException : Exception
	{
		public MongoFrameworkException() { }
		public MongoFrameworkException(string message) : base(message) { }
	}
}
