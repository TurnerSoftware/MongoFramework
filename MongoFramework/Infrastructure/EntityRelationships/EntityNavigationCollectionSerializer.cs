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
		private IDbContextSettings Settings { get; }
		private IEntityPropertyMap ForeignPropertyMap { get; }

		public string ForeignKey { get; }
		public Type ValueType => typeof(ICollection<TEntity>);

		public EntityNavigationCollectionSerializer(string foreignKey, IDbContextSettings settings)
		{
			ForeignKey = foreignKey;
			Settings = settings;

			var entityMapper = settings.GetEntityMapper<TEntity>();
			ForeignPropertyMap = entityMapper.GetEntityMapping().Where(m => m.Property.Name == foreignKey).FirstOrDefault();
		}

		public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			var type = context.Reader.GetCurrentBsonType();
			if (type == BsonType.Array)
			{
				var collection = new EntityNavigationCollection<TEntity>(ForeignKey, Settings);
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
				return new EntityNavigationCollection<TEntity>(ForeignKey, Settings);
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
				foreignIds = simpleCollection.Select(e => ForeignPropertyMap.Property.GetValue(e));
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

		public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, ICollection<TEntity> value)
		{
			Serialize(context, args, (object)value);
		}

		public bool TryGetItemSerializationInfo(out BsonSerializationInfo serializationInfo)
		{
			var serializer = BsonSerializer.LookupSerializer(ForeignPropertyMap.PropertyType);
			serializationInfo = new BsonSerializationInfo(null, serializer, ForeignPropertyMap.PropertyType);
			return true;
		}

		ICollection<TEntity> IBsonSerializer<ICollection<TEntity>>.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			return Deserialize(context, args) as ICollection<TEntity>;
		}
	}
}
