using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class EntityIdProcessor : IMappingProcessor
	{
		public void ApplyMapping(IEntityDefinition definition, BsonClassMap classMap)
		{
			IEntityProperty idProperty = default;
			foreach (var property in definition.Properties)
			{
				if (property.PropertyInfo.GetCustomAttribute<KeyAttribute>() != null)
				{
					idProperty = property;
					break;
				}
				
				if (property.ElementName.Equals("id", StringComparison.InvariantCultureIgnoreCase))
				{
					//We don't break here just in case another property has the KeyAttribute
					//We preference the attribute over the name match
					idProperty = property;
				}
			}

			if (idProperty is EntityProperty entityProperty)
			{
				classMap.MapIdMember(idProperty.PropertyInfo);
				entityProperty.IsKey = true;

				//Set an Id Generator based on the member type
				var idMemberMap = classMap.IdMemberMap;
				var memberType = BsonClassMap.GetMemberInfoType(idMemberMap.MemberInfo);
				if (memberType == typeof(string))
				{
					idMemberMap.SetIdGenerator(StringObjectIdGenerator.Instance);
					definition.KeyGenerator = new EntityKeyGenerator(StringObjectIdGenerator.Instance);
				}
				else if (memberType == typeof(Guid))
				{
					idMemberMap.SetIdGenerator(CombGuidGenerator.Instance);
					definition.KeyGenerator = new EntityKeyGenerator(CombGuidGenerator.Instance);
				}
				else if (memberType == typeof(ObjectId))
				{
					idMemberMap.SetIdGenerator(ObjectIdGenerator.Instance);
					definition.KeyGenerator = new EntityKeyGenerator(ObjectIdGenerator.Instance);
				}
			}
		}
	}
}
