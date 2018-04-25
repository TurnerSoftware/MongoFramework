using System.Collections.Generic;

namespace MongoFramework.Tests.Mapping.Processors.NestedProperty
{
	public class CollectionBaseModel
	{
		public ICollection<CollectionNestedModel> CollectionModel { get; set; }
	}
}