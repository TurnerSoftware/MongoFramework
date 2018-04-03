using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Tests.EntityRelationships.EntityCollection
{
	public class BaseEntityModel
	{
		public string Id { get; set; }
		public string Description { get; set; }

		public ICollection<RelatedEntityModel> RelatedEntities { get; set; }
	}
}
