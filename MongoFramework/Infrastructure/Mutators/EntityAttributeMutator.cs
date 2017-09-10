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
		public DbEntityMutatorType MutatorType { get; set; }

		public void MutateEntity(TEntity entity, IDbEntityMapper<TEntity> descriptor)
		{
			var mutateProperties = descriptor.GetMappedProperties().Select(p => new
			{
				PropertyInfo = p,
				MutateAttribute = p.GetCustomAttribute<MutatePropertyAttribute>()
			}).Where(p => p.MutateAttribute != null).ToList();

			foreach (var property in mutateProperties)
			{
				if (MutatorType.HasFlag(DbEntityMutatorType.Insert))
				{
					property.MutateAttribute.OnInsert(entity, property.PropertyInfo);
				}

				if (MutatorType.HasFlag(DbEntityMutatorType.Update))
				{
					property.MutateAttribute.OnUpdate(entity, property.PropertyInfo);
				}

				if (MutatorType.HasFlag(DbEntityMutatorType.Select))
				{
					property.MutateAttribute.OnSelect(entity, property.PropertyInfo);
				}
			}
		}
	}
}
