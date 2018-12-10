using MongoDB.Driver;
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
			var entityMapper = connection.GetEntityMapper(typeof(TEntity));
			var mutateProperties = entityMapper.GetEntityMapping().Select(m => new
			{
				PropertyInfo = m.Property,
				MutateAttribute = m.Property.GetCustomAttribute<MutatePropertyAttribute>(true)
			}).Where(p => p.MutateAttribute != null).ToArray();

			foreach (var property in mutateProperties)
			{
				if (mutationType == MutatorType.Insert)
				{
					property.MutateAttribute.OnInsert(entity, property.PropertyInfo);
				}

				if (mutationType == MutatorType.Update)
				{
					property.MutateAttribute.OnUpdate(entity, property.PropertyInfo);
				}

				if (mutationType == MutatorType.Select)
				{
					property.MutateAttribute.OnSelect(entity, property.PropertyInfo);
				}
			}
		}
	}
}
