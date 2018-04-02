using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;

namespace MongoFramework.Infrastructure.EntityRelationships
{
	public interface IEntityNavigationCollection
	{
		void BeginImport(IEnumerable<string> entityIds);
		void FinaliseImport(IMongoDatabase database);
	}
}
