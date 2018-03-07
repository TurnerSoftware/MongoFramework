using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoFramework.Infrastructure.DefinitionHelpers
{
	public static class UpdateDefinitionHelper
	{
		public static UpdateDefinition<TEntity> CreateFromDiff<TEntity>(BsonDocument documentA, BsonDocument documentB)
		{
			var definition = Builders<TEntity>.Update.Combine();
			return CreateFromDiff(definition, string.Empty, documentA, documentB);
		}
		private static UpdateDefinition<TEntity> CreateFromDiff<TEntity>(UpdateDefinition<TEntity> definition, string name, BsonDocument documentA, BsonDocument documentB)
		{
			var result = new BsonDocument();
			var documentAProperties = documentA?.Names ?? Enumerable.Empty<string>();
			var documentBProperties = documentB?.Names ?? Enumerable.Empty<string>();
			var propertyNames = documentAProperties.Union(documentBProperties);

			if (name != string.Empty)
			{
				name += ".";
			}

			foreach (var propertyName in propertyNames)
			{
				var fullName = name + propertyName;

				if (documentB == null || !documentB.Contains(propertyName))
				{
					definition = definition.Unset(new StringFieldDefinition<TEntity>(fullName));
				}
				else if (documentA == null || !documentA.Contains(propertyName))
				{
					definition = definition.Set(fullName, documentB[propertyName]);
				}
				else
				{
					definition = CreateFromDiff(definition, fullName, documentA[propertyName], documentB[propertyName]);
				}
			}

			return definition;
		}
		private static UpdateDefinition<TEntity> CreateFromDiff<TEntity>(UpdateDefinition<TEntity> definition, string name, BsonValue valueA, BsonValue valueB)
		{
			if (valueA == valueB)
			{
				return definition;
			}

			if (valueA == null || valueB == null || valueA.BsonType != valueB.BsonType)
			{
				definition = definition.Set(new StringFieldDefinition<TEntity, object>(name), valueB);
			}

			var bsonType = valueA.BsonType;
			if (bsonType == BsonType.Array)
			{
				return CreateFromDiff(definition, name, valueA.AsBsonArray, valueB.AsBsonArray);
			}
			else if (bsonType == BsonType.Document)
			{
				return CreateFromDiff(definition, name, valueA.AsBsonDocument, valueB.AsBsonDocument);
			}
			else if (valueA != valueB)
			{
				definition = definition.Set(name, valueB);
			}

			return definition;
		}
		private static UpdateDefinition<TEntity> CreateFromDiff<TEntity>(UpdateDefinition<TEntity> definition, string name, BsonArray arrayA, BsonArray arrayB)
		{
			var result = new BsonDocument();

			var arrayACount = arrayA.Count;
			var arrayBCount = arrayB.Count;

			for (int i = 0, l = Math.Max(arrayACount, arrayBCount); i < l; i++)
			{
				var fullName = name + "." + i;

				if (i >= arrayACount)
				{
					definition = definition.Push(new StringFieldDefinition<TEntity>(fullName), arrayB[i]);
				}
				else if (i >= arrayBCount)
				{
					definition = definition.Pull(new StringFieldDefinition<TEntity>(fullName), BsonString.Empty);
				}
				else
				{
					definition = CreateFromDiff(definition, fullName, arrayA[i], arrayB[i]);
				}
			}

			return definition;
		}
	}
}
