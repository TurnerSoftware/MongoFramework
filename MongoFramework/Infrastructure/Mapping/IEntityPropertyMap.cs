using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MongoFramework.Infrastructure.Mapping
{
	public interface IEntityPropertyMap
	{
		bool IsKey { get; }
		string ElementName { get; }
		PropertyInfo Property { get; }
	}
}
