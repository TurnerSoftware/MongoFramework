using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework
{
	public interface IMongoDbContext
	{
		void SaveChanges();
	}
}
