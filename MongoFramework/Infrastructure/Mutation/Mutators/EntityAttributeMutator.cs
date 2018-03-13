﻿using System.Linq;
using System.Reflection;
using MongoFramework.Attributes;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Infrastructure.Mutation.Mutators
{
	public class EntityAttributeMutator<TEntity> : IEntityMutator<TEntity>
	{
		public void MutateEntity(TEntity entity, MutatorType mutationType, IEntityMapper entityMapper)
		{
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
