using MongoFramework.Attributes;
using System.Collections.Generic;

namespace MongoFramework.Tests.Models
{
	public class ExtraElementsModel
	{
		public string Id { get; set; }
		[ExtraElements] public IDictionary<string, object> AdditionalElements { get; set; }
	}
}