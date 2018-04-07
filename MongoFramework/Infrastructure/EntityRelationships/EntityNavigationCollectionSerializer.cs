using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Infrastructure.EntityRelationships
{
	public class EntityNavigationCollectionSerializer<TEntity> : IBsonSerializer<ICollection<TEntity>>, IBsonArraySerializer
	{
		public Type ValueType => typeof(ICollection<TEntity>);

		public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			var type = context.Reader.GetCurrentBsonType();
			if (type == BsonType.Array)
			{
				var collection = new EntityNavigationCollection<TEntity>();
				var entityIds = new List<object>();
				context.Reader.ReadStartArray();

				while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
				{
					if (context.Reader.CurrentBsonType == BsonType.ObjectId)
					{
						var entityId = context.Reader.ReadObjectId();
						entityIds.Add(entityId);
					}
					else
					{
						var entityId = context.Reader.ReadString();
						entityIds.Add(entityId);
					}
				}

				context.Reader.ReadEndArray();
				collection.BeginImport(entityIds);
				return collection;
			}
			else if (type == BsonType.Null)
			{
				context.Reader.ReadNull();
				return new EntityNavigationCollection<TEntity>();
			}
			else
			{
				throw new NotSupportedException($"Unable to deserialize {type} into an ICollection<{typeof(TEntity).Name}>");
			}
		}

		public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
		{
			if (value is EntityNavigationCollection<TEntity> collection)
			{
				context.Writer.WriteStartArray();

				foreach (var entityId in collection.PersistingEntityIds)
				{
					if (entityId == null)
					{
						context.Writer.WriteNull();
					}
					else if (entityId is ObjectId objectId)
					{
						context.Writer.WriteObjectId(objectId);
					}
					else
					{
						context.Writer.WriteString(entityId.ToString());
					}
				}

				context.Writer.WriteEndArray();
			}
			else
			{
				context.Writer.WriteNull();
			}
		}

		public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, ICollection<TEntity> value)
		{
			Serialize(context, args, (object)value);
		}

		public bool TryGetItemSerializationInfo(out BsonSerializationInfo serializationInfo)
		{
			var serializer = BsonSerializer.LookupSerializer(typeof(string));
			var nominalType = typeof(string);
			serializationInfo = new BsonSerializationInfo(null, serializer, nominalType);
			return true;
		}

		ICollection<TEntity> IBsonSerializer<ICollection<TEntity>>.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			return Deserialize(context, args) as ICollection<TEntity>;
		}
	}
}
