using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoFramework.Infrastructure.Serialization
{
	/// <summary>
	/// Based on Luke Vosyka's implementation on CodeProject (https://www.codeproject.com/Tips/1268086/MongoDB-Csharp-Serializer-for-DateTimeOffset-to-Bs)
	/// Drops the direct DateTime serialization (as it was lossy) for a document version (date and offset stored separately)
	/// </summary>
	public class DateTimeOffsetSerializer : StructSerializerBase<DateTimeOffset>, IRepresentationConfigurable<DateTimeOffsetSerializer>
	{
		public BsonType Representation { get; }
		
		public const string StringSerializationFormat = "YYYY-MM-ddTHH:mm:ss.FFFFFFK";

		public DateTimeOffsetSerializer() : this(BsonType.Document) { }

		public DateTimeOffsetSerializer(BsonType representation)
		{
			switch (representation)
			{
				case BsonType.String:
				case BsonType.Document:
					break;
				default:
					throw new ArgumentException($"{representation} is not a valid representation for {nameof(DateTimeOffsetSerializer)}");
	
			}

			Representation = representation;
		}

		public override DateTimeOffset Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			var type = context.Reader.GetCurrentBsonType();
			if (type == BsonType.String)
			{
				var stringValue = context.Reader.ReadString();
				return DateTimeOffset.ParseExact(stringValue, StringSerializationFormat, DateTimeFormatInfo.InvariantInfo);
			}
			else if (type == BsonType.Document)
			{
				context.Reader.ReadStartDocument();
				context.Reader.SkipName();
				var dateTimeValue = context.Reader.ReadDateTime();
				context.Reader.SkipName();
				var offset = context.Reader.ReadDouble() * 60;
				var timeSpanOffset = new TimeSpan(0, (int)offset, 0);
				context.Reader.ReadEndDocument();
				return DateTimeOffset.FromUnixTimeMilliseconds(dateTimeValue).ToOffset(timeSpanOffset);
			}
			else
			{
				throw CreateCannotDeserializeFromBsonTypeException(type);
			}
		}

		public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateTimeOffset value)
		{
			var bsonWriter = context.Writer;

			switch (Representation)
			{
				case BsonType.String:
					bsonWriter.WriteString(value.ToString(StringSerializationFormat, DateTimeFormatInfo.InvariantInfo));
					break;

				case BsonType.Document:
					bsonWriter.WriteStartDocument();
					bsonWriter.WriteName("Date");
					bsonWriter.WriteDateTime(value.ToUnixTimeMilliseconds());
					bsonWriter.WriteName("Offset");
					bsonWriter.WriteDouble(value.Offset.TotalHours);
					bsonWriter.WriteEndDocument();
					break;

				default:
					throw new BsonSerializationException($"{Representation} is not a valid DateTimeOffSet representation");
			}
		}

		public DateTimeOffsetSerializer WithRepresentation(BsonType representation)
		{
			if (Representation == representation)
			{
				return this;
			}

			return new DateTimeOffsetSerializer(representation);
		}

		IBsonSerializer IRepresentationConfigurable.WithRepresentation(BsonType representation)
		{
			return WithRepresentation(representation);
		}
	}
}
