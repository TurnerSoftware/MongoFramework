using System;
using System.Collections.Generic;
using System.Text;

namespace MongoFramework.Infrastructure.Commands
{
	public class WriteModelOptions
	{
		public static WriteModelOptions Default { get; } = new WriteModelOptions();
		public string TenantId { get; set; }
	}
}
