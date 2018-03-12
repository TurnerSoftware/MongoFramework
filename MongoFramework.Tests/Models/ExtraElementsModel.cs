using System.Collections.Generic;
using MongoFramework.Attributes;

namespace MongoFramework.Tests.Models
{
	public class ExtraElementsModel
	{
		public string Id { get; set; }
		[ExtraElements] public IDictionary<string, object> AdditionalElements { get; set; }
	}
}