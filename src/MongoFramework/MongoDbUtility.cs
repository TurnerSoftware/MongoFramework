using MongoDB.Bson;

namespace MongoFramework
{
	public static class MongoDbUtility
	{
		/// <summary>
		/// Checks whether the provided string matches the 24-character hexadecimal format of an ObjectId
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static bool IsValidObjectId(string id) => ObjectId.TryParse(id, out ObjectId result);
	}
}
