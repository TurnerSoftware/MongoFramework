using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoFramework.Infrastructure.DefinitionHelpers
{
	public static class UpdateDefinitionHelper
	{
		public static UpdateDefinition<TEntity> CreateFromDiff<TEntity>(BsonDocument documentA, BsonDocument documentB) where TEntity : class
		{
			var definition = new BsonDocument();
			ApplyDiffUpdate(definition, string.Empty, documentA, documentB);
			return new BsonDocumentUpdateDefinition<TEntity>(definition);
		}
		private static void ApplyDiffUpdate(BsonDocument updateDefinition, string name, BsonDocument documentA, BsonDocument documentB)
		{
			var documentAProperties = documentA?.Names ?? Enumerable.Empty<string>();
			var documentBProperties = documentB?.Names ?? Enumerable.Empty<string>();
			var propertyNames = documentAProperties.Union(documentBProperties);

			var baseName = name;
			if (!string.IsNullOrEmpty(baseName))
			{
				baseName += ".";
			}

			foreach (var propertyName in propertyNames)
			{
				var fullName = baseName + propertyName;

				if (documentB == null || !documentB.Contains(propertyName))
				{
					ApplyPropertyUnset(updateDefinition, fullName);
				}
				else if (documentA == null || !documentA.Contains(propertyName))
				{
					ApplyPropertySet(updateDefinition, fullName, documentB[propertyName]);
				}
				else
				{
					ApplyDiffUpdate(updateDefinition, fullName, documentA[propertyName], documentB[propertyName]);
				}
			}
		}
		private static void ApplyDiffUpdate(BsonDocument updateDefinition, string name, BsonValue valueA, BsonValue valueB)
		{
			if (valueB == null)
			{
				ApplyPropertySet(updateDefinition, name, BsonNull.Value);
			}
			else if (valueA?.BsonType != valueB?.BsonType)
			{
				ApplyPropertySet(updateDefinition, name, valueB);
			}
			else
			{
				var bsonType = valueA?.BsonType;
				if (bsonType == BsonType.Array)
				{
					ApplyDiffUpdate(updateDefinition, name, valueA.AsBsonArray, valueB.AsBsonArray);
				}
				else if (bsonType == BsonType.Document)
				{
					ApplyDiffUpdate(updateDefinition, name, valueA.AsBsonDocument, valueB.AsBsonDocument);
				}
				else if (valueA != valueB)
				{
					ApplyPropertySet(updateDefinition, name, valueB);
				}
			}
		}
		private static void ApplyDiffUpdate(BsonDocument updateDefinition, string name, BsonArray arrayA, BsonArray arrayB)
		{
			var arrayACount = arrayA.Count;
			var arrayBCount = arrayB.Count;

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
					ApplyDiffUpdate(updateDefinition, fullName, arrayA[i], arrayB[i]);
				}
			}
			else
			{
				ApplyPropertySet(updateDefinition, name, arrayB);
			}
		}

		private static void ApplyPropertySet(BsonDocument updateDefinition, string name, BsonValue value)
		{
			if (updateDefinition.TryGetElement("$set", out var element))
			{
				element.Value.AsBsonDocument.Add(name, value);
			}
			else
			{
				updateDefinition.Set("$set", new BsonDocument
				{
					{ name, value }
				});
			}
		}
		private static void ApplyPropertyUnset(BsonDocument updateDefinition, string name)
		{
			if (updateDefinition.TryGetElement("$unset", out var element))
			{
				element.Value.AsBsonDocument.Add(name, 1);
			}
			else
			{
				updateDefinition.Set("$unset", new BsonDocument
				{
					{ name, 1 }
				});
			}
		}
	}
}
