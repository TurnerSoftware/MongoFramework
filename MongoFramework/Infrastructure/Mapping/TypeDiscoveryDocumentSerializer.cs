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
	public class TypeDiscoveryDocumentSerializer<TEntity> : IBsonSerializer<TEntity>, IBsonDocumentSerializer
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

				//By default, assume the document is at least the generic type
				var documentType = ValueType;

				while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
				{
					var name = context.Reader.ReadName();

					if (name == "_t")
					{
						var typeHierarchy = ReadTypeHierarchy(context);
						var topMostTypeName = typeHierarchy.LastOrDefault();
						documentType = FindTypeByName(topMostTypeName);

						if (documentType == null)
						{
							throw new TypeAccessException($"Can't find the type {topMostTypeName} in any currently loaded assemblies");
						}

						break;
					}
					else
					{
						context.Reader.SkipValue();
					}
				}

				//To deserialize the document, we need to be in the right position
				context.Reader.ReturnToBookmark(bookmark);

				var document = BsonDocumentSerializer.Instance.Deserialize(context, new BsonDeserializationArgs());
				var deserializedResult = BsonSerializer.Deserialize(document, documentType);
				return deserializedResult;
			}
			else if (type == BsonType.Null)
			{
				context.Reader.ReadNull();
				return default(TEntity);
			}
			else
			{
				throw new NotSupportedException($"Unsupported type {type} for TypeDiscoveryDocumentSerializer");
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

		private IEnumerable<string> ReadTypeHierarchy(BsonDeserializationContext context)
		{
			if (context.Reader.CurrentBsonType == BsonType.String)
			{
				return new[] { context.Reader.ReadString() };
			}
			else if (context.Reader.CurrentBsonType == BsonType.Array)
			{
				context.Reader.ReadStartArray();
				var typeList = new List<string>();

				while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
				{
					typeList.Add(context.Reader.ReadString());
				}

				context.Reader.ReadEndArray();

				return typeList;
			}

			throw new InvalidOperationException($"Unexpected reader position. Expected string or array but got {context.Reader.CurrentBsonType}.");
		}

		public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
		{
			Serialize(context, args, (TEntity)value);
		}

		public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TEntity value)
		{
			BsonSerializer.Serialize(context.Writer, ValueType, value);
		}

		public bool TryGetMemberSerializationInfo(string memberName, out BsonSerializationInfo serializationInfo)
		{
			throw new NotImplementedException();
		}

		TEntity IBsonSerializer<TEntity>.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			return (TEntity)Deserialize(context, args);
		}
	}
}
