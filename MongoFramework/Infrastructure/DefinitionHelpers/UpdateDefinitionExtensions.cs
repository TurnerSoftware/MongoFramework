using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
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
			var reflectedValueType = typeof(TEntity).GetNestedPropertyType(fieldName);

			if (valueType == null && reflectedValueType == null)
			{
				reflectedValueType = typeof(object);
			}

			if (valueType == null || (reflectedValueType != null && valueType != reflectedValueType))
			{
				valueType = reflectedValueType;
				dotNetValue = BsonSerializer.Deserialize(value.ToJson(), valueType);
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
