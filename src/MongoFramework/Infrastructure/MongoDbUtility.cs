using MongoDB.Bson;
using MongoDB.Driver;
using System;

namespace MongoFramework.Infrastructure
{
	public static class MongoDbUtility
	{
		/// <summary>
		/// Checks whether the provided string matches the 24-character hexadecimal format of an ObjectId
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static bool IsValidObjectId(string id)
		{
			return ObjectId.TryParse(id, out ObjectId result);
		}
	}
}
