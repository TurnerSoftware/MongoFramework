using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Bson
{
	public class DiffResult
	{
		public bool HasDifference { get; private set; }
		public BsonValue Difference { get; private set; }

		/// <summary>
		/// Creates a DiffResult with no differences.
		/// </summary>
		public DiffResult() { }
		/// <summary>
		/// Creates a DiffResult with the specified differences.
		/// </summary>
		/// <param name="difference"></param>
		public DiffResult(BsonValue difference)
		{
			HasDifference = true;
			Difference = difference;
		}
	}
}
