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
			else if (type == BsonType.Null)
			{
				context.Reader.ReadNull();
				return new EntityNavigationCollection<TEntity>();
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

				foreach (var entry in collection.GetEntries())
				{
					if (entry.State != DbEntityEntryState.Deleted)
					{
						var idValue = entityMapper.GetIdValue(entry.Entity);
						if (idValue != null)
						{
							context.Writer.WriteString(idValue.ToString());
						}
						else
						{
							context.Writer.WriteNull();
						}
					}
				}

				foreach (var importId in collection.ImportIds)
				{
					//Quick hack to work around that back/forth serialization of EntityNavigationCollection loses data
					context.Writer.WriteString(importId);
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
