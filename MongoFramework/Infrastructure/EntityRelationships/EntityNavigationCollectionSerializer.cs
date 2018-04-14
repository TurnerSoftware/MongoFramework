using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;

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
				context.Reader.ReadStartArray();

				while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
				{
					if (context.Reader.CurrentBsonType == BsonType.ObjectId)
					{
						var entityId = context.Reader.ReadObjectId();
						collection.AddEntityById(entityId);
					}
					else
					{
						var entityId = context.Reader.ReadString();
						collection.AddEntityById(entityId);
					}
				}

				context.Reader.ReadEndArray();
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
			if (value == null)
			{
				context.Writer.WriteNull();
				return;
			}

			IEnumerable<object> entityIds;

			if (value is EntityNavigationCollection<TEntity> entityNavigationCollection)
			{
				//Calling GetEntityIds prevents unnecessary DB calls if the entities aren't actually loaded yet
				entityIds = entityNavigationCollection.GetEntityIds();
			}
			else if (value is ICollection<TEntity> simpleCollection)
			{
				var entityMapper = new EntityMapper<TEntity>();
				entityIds = simpleCollection.Select(e => entityMapper.GetIdValue(e));
			}
			else
			{
				throw new NotSupportedException($"Unable to serialize {value.GetType().Name}. Only ICollection<{typeof(TEntity).Name}> types are supported");
			}

			context.Writer.WriteStartArray();

			foreach (var entityId in entityIds)
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

		public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, ICollection<TEntity> value)
		{
			Serialize(context, args, (object)value);
		}

		public bool TryGetItemSerializationInfo(out BsonSerializationInfo serializationInfo)
		{
			var entityMapper = new EntityMapper<TEntity>();
			var idType = entityMapper.GetEntityMapping().Where(m => m.IsKey).Select(m => m.PropertyType).FirstOrDefault();
			var serializer = BsonSerializer.LookupSerializer(idType);
			serializationInfo = new BsonSerializationInfo(null, serializer, idType);
			return true;
		}

		ICollection<TEntity> IBsonSerializer<ICollection<TEntity>>.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			return Deserialize(context, args) as ICollection<TEntity>;
		}
	}
}
