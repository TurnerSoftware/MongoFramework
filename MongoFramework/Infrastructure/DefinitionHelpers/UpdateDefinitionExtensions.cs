using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace MongoFramework.Infrastructure.DefinitionHelpers
{
	public static class UpdateDefinitionExtensions
	{
		public static UpdateDefinition<TEntity> Set<TEntity>(this UpdateDefinition<TEntity> definition, string fieldName, BsonValue value)
		{
			var dotNetValue = BsonTypeMapper.MapToDotNetValue(value);
			var valueType = dotNetValue?.GetType();
			var reflectedProperty = typeof(TEntity).GetNestedProperty(fieldName);
			var reflectedValueType = reflectedProperty?.PropertyType;

			if (valueType == null && reflectedValueType == null)
			{
				valueType = typeof(object);
			}
			else if (valueType == null || (reflectedValueType != null && valueType != reflectedValueType))
			{
				valueType = reflectedValueType;

				//Yes, this is a bit overkill
				//Basically, custom serializers weren't handled. This is problem when the actual value doesn't
				//remotely match the property type like the EntityNavigationCollection.
				//This section probably needs a bit of an overhaul anyway...
				//TODO: Cleanup this section!

				var classMap = BsonClassMap.LookupClassMap(reflectedProperty.DeclaringType);
				var memberMap = classMap.GetMemberMap(reflectedProperty.Name);
				var serializer = memberMap.GetSerializer();

				if (serializer != null)
				{
					using (var reader = new JsonReader(value.ToJson()))
					{
						var context = BsonDeserializationContext.CreateRoot(reader);
						dotNetValue = serializer.Deserialize(context);
					}
				}
				else
				{
					dotNetValue = BsonSerializer.Deserialize(value.ToJson(), valueType);
				}
			}

			var typeArgs = new[] { typeof(TEntity), valueType };

			var specificDefinitionType = typeof(StringFieldDefinition<,>).MakeGenericType(typeArgs);
			var specificDefinition = Activator.CreateInstance(specificDefinitionType, fieldName, null);

			var expressionType = typeof(Expression);
			var setMethod = typeof(MongoDB.Driver.UpdateDefinitionExtensions)
				.GetMethods()
				.Where(m => m.Name == "Set" && !m.GetParameters().Any(p => expressionType.IsAssignableFrom(p.ParameterType)))
				.FirstOrDefault()
				.MakeGenericMethod(typeArgs);

			var result = setMethod.Invoke(null, new[] { definition, specificDefinition, dotNetValue });
			return result as UpdateDefinition<TEntity>;
		}
	}
}
