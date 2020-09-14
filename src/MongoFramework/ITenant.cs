using System;
namespace MongoFramework
{
	public interface IHasTenantId
	{
		string TenantId { get; set; }
	}
}
