using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq;

namespace MongoFramework.Infrastructure.DefinitionHelpers
{
	public static class UpdateDefinitionHelper
	{
		public static UpdateDefinition<TEntity> CreateFromDiff<TEntity>(BsonDocument documentA, BsonDocument documentB) where TEntity : class
		{
			var definition = Builders<TEntity>.Update.Combine();
			return CreateFromDiff(definition, string.Empty, documentA, documentB);
		}
		private static UpdateDefinition<TEntity> CreateFromDiff<TEntity>(UpdateDefinition<TEntity> definition, string name, BsonDocument documentA, BsonDocument documentB) where TEntity : class
		{
			var documentAProperties = documentA?.Names ?? Enumerable.Empty<string>();
			var documentBProperties = documentB?.Names ?? Enumerable.Empty<string>();
			var propertyNames = documentAProperties.Union(documentBProperties);
			
			var baseName = name;
			if (!string.IsNullOrEmpty(baseName))
			{
				baseName += ".";
			}

			var resultDefinition = definition;

			foreach (var propertyName in propertyNames)
			{
				var fullName = baseName + propertyName;

				if (documentB == null || !documentB.Contains(propertyName))
				{
					resultDefinition = resultDefinition.Unset(new StringFieldDefinition<TEntity>(fullName));
				}
				else if (documentA == null || !documentA.Contains(propertyName))
				{
					resultDefinition = resultDefinition.Set(fullName, documentB[propertyName]);
				}
				else
				{
					resultDefinition = CreateFromDiff(resultDefinition, fullName, documentA[propertyName], documentB[propertyName]);
				}
			}

			return resultDefinition;
		}
		private static UpdateDefinition<TEntity> CreateFromDiff<TEntity>(UpdateDefinition<TEntity> definition, string name, BsonValue valueA, BsonValue valueB) where TEntity : class
		{
			if (valueB == null)
			{
				return definition.Set(name, BsonNull.Value);
			}
			else if (valueA?.BsonType != valueB?.BsonType)
			{
				return definition.Set(name, valueB);
			}

			var bsonType = valueA?.BsonType;
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
				return definition.Set(name, valueB);
			}

			return definition;
		}
		private static UpdateDefinition<TEntity> CreateFromDiff<TEntity>(UpdateDefinition<TEntity> definition, string name, BsonArray arrayA, BsonArray arrayB) where TEntity : class
		{
			var arrayACount = arrayA.Count;
			var arrayBCount = arrayB.Count;

			var resultDefinition = definition;

			//Due to limitations of MongoDB, we can't pull/push at the same time.
			//As highlighted on task SERVER-1014 (MongoDB Jira), you can't pull at an index, only at a value match.
			//You could avoid the pull by simply pop-ing items off the list for the length difference between "arrayA" and "arrayB".
			//That said, we can't run "conflicting" updates on the same path (eg. pull and push) at the same time.
			//Instead, if the arrays are the same length, we check differences per index.
			//If the arrays are different lengths, we set the whole array in the update.

			if (arrayACount == arrayBCount)
			{
				for (int i = 0, l = arrayBCount; i < l; i++)
				{
					var fullName = name + "." + i;
					resultDefinition = CreateFromDiff(resultDefinition, fullName, arrayA[i], arrayB[i]);
				}
			}
			else
			{
				resultDefinition = resultDefinition.Set(name, arrayB);
			}

			return resultDefinition;
		}
	}
}
