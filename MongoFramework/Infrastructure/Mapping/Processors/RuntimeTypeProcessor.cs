using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization;
using System.Linq;
using System.Reflection;
using MongoFramework.Attributes;
using System.Runtime.ExceptionServices;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class RuntimeTypeProcessor : IMappingProcessor
	{
		public void ApplyMapping(Type entityType, BsonClassMap classMap)
		{
			var runtimeDiscoveryMemberMaps = classMap.DeclaredMemberMaps.Where(m => m.MemberInfo.GetCustomAttribute<RuntimeTypeDiscoveryAttribute>() != null);

			if (entityType.GetCustomAttribute<RuntimeTypeDiscoveryAttribute>() != null)
			{
				var serializerType = typeof(TypeDiscoveryDocumentSerializer<>).MakeGenericType(entityType);
				var serializer = Activator.CreateInstance(serializerType) as IBsonSerializer;
				BsonSerializer.RegisterSerializer(entityType, serializer);
			}

			foreach (var memberMap in runtimeDiscoveryMemberMaps)
			{
				Type serializerType;
				var memberType = memberMap.MemberType;

				if (
					memberType.IsGenericType && memberType.GetGenericArguments().Count() == 1 &&
					(
						memberType.GetGenericTypeDefinition() == typeof(IEnumerable<>) || 
						memberType.GetInterfaces().Where(i => i.IsGenericType).Any(i => i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
					)
				)
				{
					var memberEntityType = memberType.GetGenericArguments().FirstOrDefault();
					serializerType = typeof(TypeDiscoveryArraySerializer<,>).MakeGenericType(memberEntityType, memberType);
				}
				else
				{
					serializerType = typeof(TypeDiscoveryDocumentSerializer<>).MakeGenericType(memberType);
				}

				try
				{
					var serializer = Activator.CreateInstance(serializerType) as IBsonSerializer;
					memberMap.SetSerializer(serializer);
				}
				catch (TargetInvocationException ex)
				{
					ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
				}
			}
		}
	}
}
