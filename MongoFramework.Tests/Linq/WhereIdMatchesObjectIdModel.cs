using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace MongoFramework.Tests.Linq
{
	public class WhereIdMatchesObjectIdModel
	{
		public ObjectId Id { get; set; }
		public string Description { get; set; }
	}
}
