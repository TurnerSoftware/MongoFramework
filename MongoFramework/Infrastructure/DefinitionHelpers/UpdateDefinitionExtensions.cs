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
				//For null values - they don't have any type data associated
				valueType = typeof(object);
			}
			else if (valueType == null || (reflectedValueType != null && valueType != reflectedValueType))
			{
				//Where BsonTypeMapper can't determine the type or it is a mismatch to what is reflected from the type
				//The preference is on the reflected type and the serializer on the specific member
				valueType = reflectedValueType;

				//We will need the serializer defined for the specific member
				//Not all serializers will be registered (eg. EntityNavigationSerializer) so we need to look it up manually
				var declaringClassMap = BsonClassMap.LookupClassMap(reflectedProperty.DeclaringType);
				var memberMap = declaringClassMap.GetMemberMap(reflectedProperty.Name);
				var serializer = memberMap.GetSerializer();

				//To prevent re-serializing back to a string, we use the BsonDocumentReader over JsonReader
				//Using BsonDocumentReader means the root must be a BsonDocument. Because the value may be a BsonArray, we need to wrap the value.
				var containerDocument = new BsonDocument
				{
					{ "Value", value }
				};

				using (var reader = new BsonDocumentReader(containerDocument))
				{
					//Get the reader into a state where the serializer is starting on the right element
					reader.ReadBsonType();
					reader.ReadStartDocument();
					reader.ReadBsonType();
					reader.SkipName();

					var context = BsonDeserializationContext.CreateRoot(reader);
					dotNetValue = serializer.Deserialize(context);
				}
			}

			var typeArgs = new[] { typeof(TEntity), valueType };

			var specificDefinitionType = typeof(StringFieldDefinition<,>).MakeGenericType(typeArgs);
			var specificDefinition = Activator.CreateInstance(specificDefinitionType, fieldName, null); //ie. StringFieldDefintion<TEntity, valueType>
			
			var expressionType = typeof(Expression);
			var setMethod = typeof(MongoDB.Driver.UpdateDefinitionExtensions)
				.GetMethods()
				.Where(m => m.Name == "Set" && !m.GetParameters().Any(p => expressionType.IsAssignableFrom(p.ParameterType)))
				.FirstOrDefault()
				.MakeGenericMethod(typeArgs);

			//Breaking down the above reflection, we are trying to get the "Set" method that doesn't take an "Expression" as a parameter
			//The method we want takes an `UpdateDefinition`, `FieldDefinition` and `Value` as the 3 parameters
			//Accounting for the variables and types, the method call would look something, something like the following
			//MongoDB.Driver.UpdateDefinitionExtensions.Set<TEntity, TField>(UpdateDefinition<TEntity> definition, StringFieldDefinition<TEntity, TField> specificDefinition, dotNetValue)

			var result = setMethod.Invoke(null, new[] { definition, specificDefinition, dotNetValue });
			return result as UpdateDefinition<TEntity>;
		}

		public static bool HasChanges<TEntity>(this UpdateDefinition<TEntity> definition)
		{
			var serializerRegistry = BsonSerializer.SerializerRegistry;
			var documentSerializer = serializerRegistry.GetSerializer<TEntity>();
			var renderedDefinition = definition.Render(documentSerializer, serializerRegistry);
			return !renderedDefinition.Equals(new BsonDocument());
		}
	}
}
