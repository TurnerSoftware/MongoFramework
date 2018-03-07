using MongoDB.Bson;
using System;
using System.Linq;

namespace MongoFramework.Bson
{
	public class BsonDiff
	{
		public static bool HasDifferences(BsonDocument documentA, BsonDocument documentB)
		{
			if (documentA == null && documentB == null)
			{
				return false;
			}
			else if (documentA == null || documentB == null || documentA.ElementCount != documentB.ElementCount)
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
			if (valueA == null && valueB == null)
			{
				return false;
			}
			else if (valueA != valueB || valueA.BsonType != valueB.BsonType)
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
			if (arrayA == null && arrayB == null)
			{
				return false;
			}
			else if (arrayA == null || arrayB == null || arrayA.Count != arrayB.Count)
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
			var documentAProperties = documentA?.Names ?? Enumerable.Empty<string>();
			var documentBProperties = documentB?.Names ?? Enumerable.Empty<string>();
			var propertyNames = documentAProperties.Union(documentBProperties);

			foreach (var propertyName in propertyNames)
			{
				if (documentB == null || !documentB.Contains(propertyName))
				{
					result.Add(propertyName, BsonUndefined.Value);
				}
				else if (documentA == null || !documentA.Contains(propertyName))
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

			var arrayACount = arrayA?.Count ?? 0;
			var arrayBCount = arrayB?.Count ?? 0;

			for (int i = 0, l = Math.Max(arrayACount, arrayBCount); i < l; i++)
			{
				if (i >= arrayACount)
				{
					result[i.ToString()] = arrayB[i];
				}
				else if (i >= arrayBCount)
				{
					result[i.ToString()] = BsonUndefined.Value;
				}
				else
				{
					var diffResult = GetDifferences(arrayA[i], arrayB[i]);
					if (diffResult.HasDifference)
					{
						result[i.ToString()] = diffResult.Difference;
					}
				}
			}

			if (result.ElementCount > 0)
			{
				return new DiffResult(result);
			}

			return new DiffResult();
		}
	}
}
