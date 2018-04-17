using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoFramework.Infrastructure.Mapping
{
	public class TypeDiscoveryArraySerializer<TEntity> : IBsonSerializer, IBsonArraySerializer
	{
		public Type ValueType => typeof(ICollection<TEntity>);

		public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			var type = context.Reader.GetCurrentBsonType();
			if (type == BsonType.Array)
			{
				var documentSerializer = new TypeDiscoveryDocumentSerializer<TEntity>();
				var collection = new List<TEntity>();
				context.Reader.ReadStartArray();

				while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
				{
					if (context.Reader.CurrentBsonType == BsonType.Document)
					{
						var result = documentSerializer.Deserialize(context);
						collection.Add(result);
					}
					else if (context.Reader.CurrentBsonType == BsonType.Null)
					{
						collection.Add(default(TEntity));
					}
					else
					{
						context.Reader.SkipValue();
					}
				}

				context.Reader.ReadEndArray();
				return collection;
			}
			else if (type == BsonType.Null)
			{
				return default(ICollection<TEntity>);
			}
			else
			{
				throw new NotSupportedException($"Unsupported type {type} for TypeDiscoveryArraySerializer");
			}
		}

		public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
		{
			BsonSerializer.Serialize(context.Writer, ValueType, value);
		}

		public bool TryGetItemSerializationInfo(out BsonSerializationInfo serializationInfo)
		{
			throw new NotImplementedException();
		}
	}
}
