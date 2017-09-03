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
	public interface IDbEntityDescriptor
	{
		void ConfigureEntity();
	}

	public interface IDbEntityMapper<TEntity> : IDbEntityDescriptor
	{
		BsonClassMap<TEntity> ClassMap { get; }
		string GetCollectionName();
		string GetEntityIdName();
		object GetEntityIdValue(TEntity entity);
		IEnumerable<PropertyInfo> GetMappedProperties();
	}
}
