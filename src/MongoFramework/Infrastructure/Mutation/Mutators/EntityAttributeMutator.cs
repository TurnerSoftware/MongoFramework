using MongoFramework.Attributes;
using MongoFramework.Infrastructure.Mapping;
using System.Linq;
using System.Reflection;

namespace MongoFramework.Infrastructure.Mutation.Mutators
{
	public class EntityAttributeMutator<TEntity> : IEntityMutator<TEntity> where TEntity : class
	{
		public void MutateEntity(TEntity entity, MutatorType mutationType, IMongoDbConnection connection)
		{
			var definition = EntityMapping.GetOrCreateDefinition(typeof(TEntity));
			var mutatePropertiesMap = definition.GetAllProperties().Select(p => new
			{
				Property = p,
				MutateAttribute = p.PropertyInfo.GetCustomAttribute<MutatePropertyAttribute>(true)
			}).Where(p => p.MutateAttribute != null).ToArray();

			foreach (var propertyMap in mutatePropertiesMap)
			{
				if (mutationType == MutatorType.Insert)
				{
					propertyMap.MutateAttribute.OnInsert(entity, propertyMap.Property);
				}

				if (mutationType == MutatorType.Update)
				{
					propertyMap.MutateAttribute.OnUpdate(entity, propertyMap.Property);
				}

				if (mutationType == MutatorType.Select)
				{
					propertyMap.MutateAttribute.OnSelect(entity, propertyMap.Property);
				}
			}
		}
	}
}
