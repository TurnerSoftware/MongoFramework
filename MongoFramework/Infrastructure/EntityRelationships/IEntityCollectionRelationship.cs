using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;

namespace MongoFramework.Infrastructure.EntityRelationships
{
	public interface IEntityCollectionRelationship
	{
		void BeginImport(IEnumerable<string> entityIds);
		void FinaliseImport(IMongoDatabase database);
	}
}
