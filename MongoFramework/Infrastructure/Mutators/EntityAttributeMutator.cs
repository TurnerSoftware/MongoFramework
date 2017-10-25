using MongoFramework.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.Mutators
{
	public class EntityAttributeMutator<TEntity> : IDbEntityMutator<TEntity>
	{
		public void MutateEntity(TEntity entity, DbEntityMutatorType mutationType, IDbEntityMapper entityMapper)
		{
			var mappedProperties = entityMapper.GetMappedProperties();
			var selected = mappedProperties.Select(p => new
			{
				PropertyInfo = p,
				MutateAttribute = p.GetCustomAttribute<MutatePropertyAttribute>(true),
				Attributes = p.GetCustomAttributes()
			});

			var mutateProperties = entityMapper.GetMappedProperties().Select(p => new
			{
				PropertyInfo = p,
				MutateAttribute = p.GetCustomAttribute<MutatePropertyAttribute>(true)
			}).Where(p => p.MutateAttribute != null).ToList();

			foreach (var property in mutateProperties)
			{
				if (mutationType == DbEntityMutatorType.Insert)
				{
					property.MutateAttribute.OnInsert(entity, property.PropertyInfo);
				}

				if (mutationType == DbEntityMutatorType.Update)
				{
					property.MutateAttribute.OnUpdate(entity, property.PropertyInfo);
				}

				if (mutationType == DbEntityMutatorType.Select)
				{
					property.MutateAttribute.OnSelect(entity, property.PropertyInfo);
				}
			}
		}
	}
}
