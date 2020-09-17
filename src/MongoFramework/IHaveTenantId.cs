using System;
namespace MongoFramework
{
	public interface IHaveTenantId
	{
		string TenantId { get; set; }
	}
}
