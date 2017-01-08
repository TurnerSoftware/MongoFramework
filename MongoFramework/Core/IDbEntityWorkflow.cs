using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Core
{
	public interface IDbEntityWorkflow
	{
		void ConfigureEntity();
		void ConfigureEntityId();
		void ConfigureMappedFields();
		void ConfigureExtraElements();
		void ConfigureSubProperties();
	}

	public interface IDbEntityWorkflow<TEntity> : IDbEntityWorkflow
	{
		BsonClassMap<TEntity> ClassMap { get; }
		string GetCollectionName();
		string GetEntityIdName();
		object GetEntityIdValue(TEntity entity);
	}
}
