using MongoFramework.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Tests
{
	class TestDbContext : MongoDbContext
	{
		public TestDbContext(string connectionName) : base(connectionName) { }
		public TestDbContext(string connectionString, string databaseName) : base(connectionString, databaseName) { }

		public MongoDbSet<AttributeEntity> AttributeEntities { get; set; }
		public MongoDbSet<CommonEntity> CommonEntities { get; set; }
		public MongoDbSet<ExtendedEntity> ExtendEntities { get; set; }
	}
}
