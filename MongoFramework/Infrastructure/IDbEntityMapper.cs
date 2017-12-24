using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Mutation;
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
		Type EntityType { get; }
		string GetIdName();
		object GetIdValue(object entity);
		string GetCollectionName();
		IEnumerable<IDbEntityPropertyMap> GetEntityMapping();
	}
}
