using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MongoFramework.Infrastructure
{
	internal class EntityPropertyMap : IDbEntityPropertyMap
	{
		public bool IsKey { get; set; }
		public string ElementName { get; set; }
		public PropertyInfo Property { get; set; }
	}
}
