using System;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using System.Diagnostics;
using MongoFramework.Infrastructure.Serialization;

namespace MongoFramework.Infrastructure;

/// <summary>
/// Provides a single entry point to configure common areas of the driver for MongoFramework
/// </summary>
internal static class DriverAbstractionRules
{
	public static void ApplyRules()
	{
		RegisterSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128));
		RegisterSerializer<decimal?>(new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));

		// MongoDB.Driver 3.x requires explicit GuidRepresentation (default is Unspecified which throws)
		// Use Standard (UUID binary subtype 4) for cross-platform compatibility
		RegisterSerializer<Guid>(new GuidSerializer(GuidRepresentation.Standard));
		RegisterSerializer<Guid?>(new NullableSerializer<Guid>(new GuidSerializer(GuidRepresentation.Standard)));

		BsonSerializer.RegisterSerializationProvider(TypeDiscoverySerializationProvider.Instance);
	}

	private static void RegisterSerializer<TTarget>(IBsonSerializer serializer)
	{
		try
		{
			BsonSerializer.RegisterSerializer(typeof(TTarget), serializer);
		}
		catch (BsonSerializationException ex) when (ex.Message.Contains("already a serializer registered"))
		{
			// Already registered
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message, "MongoFramework");
		}
	}
}
