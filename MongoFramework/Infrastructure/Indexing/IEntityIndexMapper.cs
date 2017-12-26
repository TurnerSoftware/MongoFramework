using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.Indexing
{
	public interface IEntityIndexMapper
	{
		Type EntityType { get; }
		IEnumerable<IEntityIndexMap> GetIndexMapping();
	}
}
