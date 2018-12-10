using System;
using System.Collections.Generic;
using System.Text;

namespace MongoFramework.Attributes
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class RuntimeTypeDiscoveryAttribute : Attribute
	{

	}
}
