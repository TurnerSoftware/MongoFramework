namespace MongoFramework.Infrastructure.Indexing
{
	internal class EntityIndexMap : IEntityIndexMap
	{
		public string ElementName { get; set; }
		public string FullPath { get; set; }
		public IEntityIndex Index { get; set; }
	}
}
