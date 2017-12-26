using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.Mapping
{
	public interface IEntityMapper
	{
		Type EntityType { get; }
		string GetIdName();
		object GetIdValue(object entity);
		string GetCollectionName();
		IEnumerable<IEntityPropertyMap> GetEntityMapping();
		IEnumerable<IEntityPropertyMap> TraverseMapping();
	}
}
