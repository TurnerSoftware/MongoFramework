﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MongoFramework.Infrastructure.Serialization
{
	public class EntityNavigationCollectionSerializer<TEntity> : IBsonSerializer<ICollection<TEntity>>, IBsonArraySerializer where TEntity : class
	{
		public IEntityProperty ForeignProperty { get; }
		public Type ValueType => typeof(ICollection<TEntity>);
		
		public EntityNavigationCollectionSerializer(IEntityProperty foreignProperty) => ForeignProperty = foreignProperty;
		public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			var type = context.Reader.GetCurrentBsonType();
			if (type == BsonType.Array)
			{
				var collection = new EntityNavigationCollection<TEntity>(ForeignProperty);
				context.Reader.ReadStartArray();

				while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
				{
					if (context.Reader.CurrentBsonType == BsonType.ObjectId)
					{
						var entityId = context.Reader.ReadObjectId();
						collection.AddForeignId(entityId);
					}
					else if (context.Reader.CurrentBsonType == BsonType.String)
					{
						var entityId = context.Reader.ReadString();
						collection.AddForeignId(entityId);
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
				context.Reader.ReadNull();
				return new EntityNavigationCollection<TEntity>(ForeignProperty);
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

			IEnumerable<object> foreignIds;

			if (value is EntityNavigationCollection<TEntity> entityNavigationCollection)
			{
				foreignIds = entityNavigationCollection.GetForeignIds();
			}
			else if (value is ICollection<TEntity> simpleCollection)
			{
				foreignIds = simpleCollection.Select(e => ForeignProperty.PropertyInfo.GetValue(e));
			}
			else
			{
				throw new NotSupportedException($"Unable to serialize {value.GetType().Name}. Only ICollection<{typeof(TEntity).Name}> types are supported");
			}

			context.Writer.WriteStartArray();

			foreach (var foreignId in foreignIds)
			{
				if (foreignId == null)
				{
					context.Writer.WriteNull();
				}
				else if (foreignId is ObjectId objectId)
				{
					context.Writer.WriteObjectId(objectId);
				}
				else
				{
					context.Writer.WriteString(foreignId.ToString());
				}
			}

			context.Writer.WriteEndArray();
		}

		public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, ICollection<TEntity> value) => Serialize(context, args, (object)value);

		public bool TryGetItemSerializationInfo(out BsonSerializationInfo serializationInfo)
		{
			var serializer = BsonSerializer.LookupSerializer(ForeignProperty.PropertyType);
			serializationInfo = new BsonSerializationInfo(null, serializer, ForeignProperty.PropertyType);
			return true;
		}

		ICollection<TEntity> IBsonSerializer<ICollection<TEntity>>.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args) => Deserialize(context, args) as ICollection<TEntity>;
	}
}
