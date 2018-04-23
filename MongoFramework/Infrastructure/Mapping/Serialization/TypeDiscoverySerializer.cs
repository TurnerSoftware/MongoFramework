using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace MongoFramework.Infrastructure.Mapping.Serialization
{
	public class TypeDiscoverySerializer<TEntity> : IBsonSerializer<TEntity>, IBsonDocumentSerializer, IBsonIdProvider
	{
		private static ReaderWriterLockSlim TypeCacheLock { get; } = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
		private static ConcurrentBag<Type> AssignableTypes { get; } = new ConcurrentBag<Type>();

		public Type ValueType => typeof(TEntity);

		public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			var type = context.Reader.GetCurrentBsonType();
			if (type == BsonType.Document)
			{
				var bookmark = context.Reader.GetBookmark();
				context.Reader.ReadStartDocument();

				var actualType = ValueType;
				if (context.Reader.FindElement("_t"))
				{
					var discriminator = BsonValueSerializer.Instance.Deserialize(context);
					if (discriminator.IsBsonArray)
					{
						discriminator = discriminator.AsBsonArray.Last();
					}
					if (discriminator.IsString)
					{
						actualType = FindTypeByName(discriminator.AsString);
					}
				}
				context.Reader.ReturnToBookmark(bookmark);

				var serializer = GetRealSerializer(actualType);
				var deserializedResult = serializer.Deserialize(context);
				return deserializedResult;
			}
			else if (type == BsonType.Null)
			{
				context.Reader.ReadNull();
				return default(TEntity);
			}
			else
			{
				throw new NotSupportedException($"Unsupported BsonType {type} for TypeDiscoverySerializer");
			}
		}

		private Type FindTypeByName(string name)
		{
			TypeCacheLock.EnterUpgradeableReadLock();

			try
			{
				var cachedType = AssignableTypes.Where(t => t.Name == name).FirstOrDefault();

				if (cachedType == null)
				{
					TypeCacheLock.EnterWriteLock();
					try
					{
						var assignableType = AppDomain.CurrentDomain.GetAssemblies()
							.Where(a => !a.IsDynamic)
							.SelectMany(a => a.GetTypes())
							.Where(t => t.Name == name && typeof(TEntity).IsAssignableFrom(t))
							.FirstOrDefault();

						AssignableTypes.Add(assignableType);

						return assignableType;
					}
					finally
					{
						TypeCacheLock.ExitWriteLock();
					}
				}

				return cachedType;
			}
			finally
			{
				TypeCacheLock.ExitUpgradeableReadLock();
			}
		}

		private IBsonSerializer GetRealSerializer(Type type)
		{
			//Force the type to be processed by the Entity Mapper
			new EntityMapper(type);

			var classMap = BsonClassMap.LookupClassMap(type);
			var serializerType = typeof(BsonClassMapSerializer<>).MakeGenericType(type);
			var serializer = (IBsonSerializer)Activator.CreateInstance(serializerType, classMap);
			return serializer;
		}

		public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
		{
			Serialize(context, args, (TEntity)value);
		}

		public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TEntity value)
		{
			var serializer = GetRealSerializer(value?.GetType() ?? ValueType);
			serializer.Serialize(context, args, value);
		}

		public bool TryGetMemberSerializationInfo(string memberName, out BsonSerializationInfo serializationInfo)
		{
			var classMap = BsonClassMap.GetRegisteredClassMaps().Where(c => c.ClassType == typeof(TEntity)).FirstOrDefault();
			//Serializer requires frozen class map
			classMap.Freeze();
			return new BsonClassMapSerializer<TEntity>(classMap).TryGetMemberSerializationInfo(memberName, out serializationInfo);
		}

		TEntity IBsonSerializer<TEntity>.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			return (TEntity)Deserialize(context, args);
		}

		public bool GetDocumentId(object document, out object id, out Type idNominalType, out IIdGenerator idGenerator)
		{
			//This specifically DOESN'T look at the type of the given document as it assumes the known document has the Id defined
			var classMap = BsonClassMap.GetRegisteredClassMaps().Where(c => c.ClassType == typeof(TEntity)).FirstOrDefault();
			//Serializer requires frozen class map
			classMap.Freeze();
			return new BsonClassMapSerializer<TEntity>(classMap).GetDocumentId(document, out id, out idNominalType, out idGenerator);
		}

		public void SetDocumentId(object document, object id)
		{
			//This specifically DOESN'T look at the type of the given document as it assumes the known document has the Id defined
			var classMap = BsonClassMap.GetRegisteredClassMaps().Where(c => c.ClassType == typeof(TEntity)).FirstOrDefault();
			//Serializer requires frozen class map
			classMap.Freeze();
			new BsonClassMapSerializer<TEntity>(classMap).SetDocumentId(document, id);
		}
	}
}
