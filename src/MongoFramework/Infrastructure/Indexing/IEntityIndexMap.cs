namespace MongoFramework.Infrastructure.Indexing
{
	public interface IEntityIndexMap
	{
		string ElementName { get; }
		string FullPath { get; }
		IEntityIndex Index { get; }
	}
}
