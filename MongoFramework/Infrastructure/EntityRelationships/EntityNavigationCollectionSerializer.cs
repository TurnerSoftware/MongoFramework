using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Infrastructure.EntityRelationships
{
	public class EntityNavigationCollectionSerializer<TEntity> : IBsonSerializer, IBsonArraySerializer
	{
		public Type ValueType => typeof(EntityNavigationCollection<TEntity>);

		public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			var type = context.Reader.GetCurrentBsonType();
			if (type == BsonType.Array)
			{
				var collection = new EntityNavigationCollection<TEntity>();
				var importIds = new List<string>();
				context.Reader.ReadStartArray();

				while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
				{
					var entityId = context.Reader.ReadString();
					importIds.Add(entityId);
				}

				context.Reader.ReadEndArray();
				collection.BeginImport(importIds);
				return collection;
			}
			else
			{
				throw new NotImplementedException($"Unable to deserialize {type} into an ICollection<{typeof(TEntity).Name}>");
			}
		}

		public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
		{
			if (value is EntityNavigationCollection<TEntity> collection)
			{
				var entityMapper = new EntityMapper<TEntity>();
				context.Writer.WriteStartArray();

				foreach (var entity in collection)
				{
					var idValue = entityMapper.GetIdValue(entity);
					context.Writer.WriteString(idValue.ToString());
				}

				context.Writer.WriteEndArray();
			}
		}

		public bool TryGetItemSerializationInfo(out BsonSerializationInfo serializationInfo)
		{
			var serializer = BsonSerializer.LookupSerializer(typeof(string));
			var nominalType = typeof(string);
			serializationInfo = new BsonSerializationInfo(null, serializer, nominalType);
			return true;
		}
	}
}
