using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class EntityIdProcessor : IMappingProcessor
	{
		public void ApplyMapping(Type entityType, BsonClassMap classMap)
		{
			//If no Id member map exists, find the first property with the "Key" attribute or is named "Id" and use that
			if (classMap.IdMemberMap == null)
			{
				var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				var idProperty = properties.Where(p => p.GetCustomAttribute<KeyAttribute>() != null).FirstOrDefault();
				if (idProperty != null)
				{
					classMap.MapIdMember(idProperty);
				}
			}

			//If there is no Id generator, set a default based on the member type
			if (classMap.IdMemberMap != null && classMap.IdMemberMap.IdGenerator == null)
			{
				var idMemberMap = classMap.IdMemberMap;
				var memberType = BsonClassMap.GetMemberInfoType(idMemberMap.MemberInfo);
				if (memberType == typeof(string))
				{
					idMemberMap.SetIdGenerator(StringObjectIdGenerator.Instance);
				}
				else if (memberType == typeof(Guid))
				{
					idMemberMap.SetIdGenerator(CombGuidGenerator.Instance);
				}
				else if (memberType == typeof(ObjectId))
				{
					idMemberMap.SetIdGenerator(ObjectIdGenerator.Instance);
				}
			}
		}
	}
}
