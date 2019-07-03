using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace MongoFramework.Infrastructure.Linq
{
	public class AggregateExecutionModel
	{
		public IEnumerable<BsonDocument> Stages { get; set; }

		public IBsonSerializer Serializer { get; set; }

		public LambdaExpression ResultTransformer { get; set; }
	}
}
