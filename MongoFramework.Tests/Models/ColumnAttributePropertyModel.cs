using System.ComponentModel.DataAnnotations.Schema;

namespace MongoFramework.Tests.Models
{
	public class ColumnAttributePropertyModel
	{
		public string Id { get; set; }
		[Column("CustomPropertyName")]
		public string MyProperty { get; set; }
	}
}