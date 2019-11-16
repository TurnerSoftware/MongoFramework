using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Linq;
using MongoFramework.Infrastructure.Mapping;
using System.Reflection;

namespace MongoFramework.Infrastructure.DefinitionHelpers
{
	public static class UpdateDefinitionExtensions
	{
		public static UpdateDefinition<TEntity> Set<TEntity>(this UpdateDefinition<TEntity> definition, string fieldName, BsonValue value) where TEntity : class
		{
			var dotNetValue = BsonTypeMapper.MapToDotNetValue(value);
			var valueType = dotNetValue?.GetType();

			IEntityProperty propertyDefinition = null;
			if (EntityMapping.IsValidTypeToMap(typeof(TEntity)))
			{
				propertyDefinition = EntityMapping.GetOrCreateDefinition(typeof(TEntity))
					.TraverseProperties()
					.Where(p => p.FullPath == fieldName)
					.FirstOrDefault();
			}

			var propertyType = propertyDefinition?.PropertyType;

			if (valueType == null && propertyType == null)
			{
				//For null values - they don't have any type data associated
				valueType = typeof(object);
			}
			else if (valueType == null || (propertyType != null && valueType != propertyType))
			{
				//Where BsonTypeMapper can't determine the type or it is a mismatch to what is set in the entity definition
				//The preference is on the definition type and the serializer on the specific member
				valueType = propertyType;

				//We will need the serializer defined for the specific member
				//Not all serializers will be registered (eg. EntityNavigationSerializer) so we need to look it up manually
				var declaringClassMap = BsonClassMap.LookupClassMap(propertyDefinition.EntityType);
				var memberMap = declaringClassMap.GetMemberMapForElement(propertyDefinition.ElementName);
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

			foreach (var method in typeof(UpdateDefinitionExtensions).GetMethods(BindingFlags.NonPublic | BindingFlags.Static))
			{
				if (method.Name == nameof(InternalSet))
				{
					var internalSetMethod = method.MakeGenericMethod(typeArgs);
					var result = internalSetMethod.Invoke(null, new[] { definition, specificDefinition, dotNetValue });
					return result as UpdateDefinition<TEntity>;
				}
			}

			return default;
		}

#pragma warning disable CRR0026 // Unused member - used via reflection
		/// <summary>
		/// Prevents reflection to the third-party library
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <typeparam name="TField"></typeparam>
		/// <param name="definition"></param>
		/// <param name="field"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private static UpdateDefinition<TEntity> InternalSet<TEntity, TField>(UpdateDefinition<TEntity> definition, FieldDefinition<TEntity, TField> field, TField value) where TEntity : class
		{
			return definition.Set(field, value);
		}
#pragma warning restore CRR0026 // Unused member - used via reflection

		public static bool HasChanges<TEntity>(this UpdateDefinition<TEntity> definition)
		{
			var serializerRegistry = BsonSerializer.SerializerRegistry;
			var documentSerializer = serializerRegistry.GetSerializer<TEntity>();
			var renderedDefinition = definition.Render(documentSerializer, serializerRegistry);
			return !renderedDefinition.Equals(new BsonDocument());
		}
	}
}
