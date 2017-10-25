using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Mutators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure
{
	public interface IDbEntityMapper
	{
		string GetIdName();
		object GetIdValue(object entity);
		string GetCollectionName();
		IEnumerable<PropertyInfo> GetMappedProperties();
	}
}
