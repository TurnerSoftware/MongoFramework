using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Bson
{
	public static class BsonDiff
	{
		public static bool HasDifferences(BsonDocument documentA, BsonDocument documentB)
		{
			if (documentA.ElementCount != documentB.ElementCount)
			{
				return true;
			}

			var propertyNames = documentA.Names.Union(documentB.Names);

			foreach (var propertyName in propertyNames)
			{
				if (!documentB.Contains(propertyName))
				{
					return true;
				}
				else if (!documentA.Contains(propertyName))
				{
					return true;
				}
				else
				{
					var propertyHasDifference = HasDifferences(documentA[propertyName], documentB[propertyName]);
					if (propertyHasDifference)
					{
						return true;
					}
				}
			}
			return false;
		}
		public static bool HasDifferences(BsonValue valueA, BsonValue valueB)
		{
			if (valueA != valueB || valueA.BsonType != valueB.BsonType)
			{
				return true;
			}

			var bsonType = valueA.BsonType;
			if (bsonType == BsonType.Array)
			{
				return HasDifferences(valueA.AsBsonArray, valueB.AsBsonArray);
			}
			else if (bsonType == BsonType.Document)
			{
				return HasDifferences(valueA.AsBsonDocument, valueB.AsBsonDocument);
			}

			return false;
		}
		public static bool HasDifferences(BsonArray arrayA, BsonArray arrayB)
		{
			if (arrayA.Count != arrayB.Count)
			{
				return true;
			}
			
			for (int i = 0, l = arrayA.Count; i < l; i++)
			{
				var itemHasDifference = HasDifferences(arrayA[i], arrayB[i]);
				if (itemHasDifference)
				{
					return true;
				}
			}

			return false;
		}

		public static DiffResult GetDifferences(BsonDocument documentA, BsonDocument documentB)
		{
			var result = new BsonDocument();
			var propertyNames = documentA.Names.Union(documentB.Names);

			foreach (var propertyName in propertyNames)
			{
				if (!documentB.Contains(propertyName))
				{
					result.Add(propertyName, BsonUndefined.Value);
				}
				else if (!documentA.Contains(propertyName))
				{
					result.Add(propertyName, documentB[propertyName]);
				}
				else
				{
					var diffResult = GetDifferences(documentA[propertyName], documentB[propertyName]);
					if (diffResult.HasDifference)
					{
						result.Add(propertyName, diffResult.Difference);
					}
				}
			}

			if (result.ElementCount > 0)
			{
				return new DiffResult(result);
			}

			return new DiffResult();
		}
		public static DiffResult GetDifferences(BsonValue valueA, BsonValue valueB)
		{
			if (valueA == valueB)
			{
				return new DiffResult();
			}

			if (valueA == null || valueB == null || valueA.BsonType != valueB.BsonType)
			{
				return new DiffResult(valueB);
			}

			var bsonType = valueA.BsonType;
			if (bsonType == BsonType.Array)
			{
				return GetDifferences(valueA.AsBsonArray, valueB.AsBsonArray);
			}
			else if (bsonType == BsonType.Document)
			{
				return GetDifferences(valueA.AsBsonDocument, valueB.AsBsonDocument);
			}
			else if (valueA != valueB)
			{
				return new DiffResult(valueB);
			}
			else
			{
				return new DiffResult();
			}
		}
		public static DiffResult GetDifferences(BsonArray arrayA, BsonArray arrayB)
		{
			var result = new BsonDocument();

			var arrayACount = arrayA.Count;
			var arrayBCount = arrayB.Count;

			for (int i = 0, l = Math.Max(arrayACount, arrayBCount); i < l; i++)
			{
				if (i >= arrayACount)
				{
					result[i] = arrayB[i];
				}
				else if (i >= arrayBCount)
				{
					result[i] = BsonUndefined.Value;
				}
				else
				{
					var diffResult = GetDifferences(arrayA[i], arrayB[i]);
					if (diffResult.HasDifference)
					{
						result[i] = diffResult.Difference;
					}
				}
			}

			if (result.ElementCount > 0)
			{
				return new DiffResult(result);
			}

			return new DiffResult();
		}

		public static UpdateDefinition<TEntity> GetUpdateDefinition<TEntity>(BsonDocument documentA, BsonDocument documentB)
		{
			var definition = Builders<TEntity>.Update.Combine();
			return GetUpdateDefinition(definition, string.Empty, documentA, documentB);
		}
		private static UpdateDefinition<TEntity> GetUpdateDefinition<TEntity>(UpdateDefinition<TEntity> definition, string name, BsonDocument documentA, BsonDocument documentB)
		{
			var result = new BsonDocument();
			var propertyNames = documentA.Names.Union(documentB.Names);

			if (name != string.Empty)
			{
				name += ".";
			}

			foreach (var propertyName in propertyNames)
			{
				var fullName = name + propertyName;

				if (!documentB.Contains(propertyName))
				{
					definition = definition.Unset(new StringFieldDefinition<TEntity>(fullName));
				}
				else if (!documentA.Contains(propertyName))
				{
					definition = definition.Set(fullName, documentB[propertyName]);
				}
				else
				{
					definition = GetUpdateDefinition(definition, fullName, documentA[propertyName], documentB[propertyName]);
				}
			}
			
			return definition;
		}
		private static UpdateDefinition<TEntity> GetUpdateDefinition<TEntity>(UpdateDefinition<TEntity> definition, string name, BsonValue valueA, BsonValue valueB)
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
				return GetUpdateDefinition(definition, name, valueA.AsBsonArray, valueB.AsBsonArray);
			}
			else if (bsonType == BsonType.Document)
			{
				return GetUpdateDefinition(definition, name, valueA.AsBsonDocument, valueB.AsBsonDocument);
			}
			else if (valueA != valueB)
			{
				definition = definition.Set(name, valueB);
			}

			return definition;
		}
		private static UpdateDefinition<TEntity> GetUpdateDefinition<TEntity>(UpdateDefinition<TEntity> definition, string name, BsonArray arrayA, BsonArray arrayB)
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
					definition = GetUpdateDefinition(definition, fullName, arrayA[i], arrayB[i]);
				}
			}

			return definition;
		}
	}
}
