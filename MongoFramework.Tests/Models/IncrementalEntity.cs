using MongoFramework.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Tests.Models
{
	class IncrementalEntity
	{
		[IncrementNumber]
		public int ByDefault { get; set; }
		[IncrementNumber(true)]
		public int ByUpdateOnly { get; set; }
		[IncrementNumber(10)]
		public int ByTen { get; set; }
	}
}
