using System;
using System.Net;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace MongoFramework.Benchmarks
{
	[SimpleJob(RuntimeMoniker.Net50), MemoryDiagnoser]
	public class SerializationComparisonBenchmark
	{
		class ExampleClass
		{
			public string Id { get; set; }
			public string Name { get; set; }
			public string Description { get; set; }
			public DateTime StartDate { get; set; }
			public DateTime? EndDate { get; set; }
			public int Count { get; set; }
			public bool IsEnabled { get; set; }
			public NestedObject Data { get; set; }
		}

		class NestedObject
		{
			public HttpStatusCode StatusCode { get; set; }
			public double Time { get; set; }
			public string[] Items { get; set; }
		}

		class ExampleCustomSerializer : IBsonSerializer<ExampleClass>
		{
			public Type ValueType => typeof(ExampleClass);

			public ExampleClass Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
			{
				throw new NotImplementedException();
			}

			public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, ExampleClass value)
			{
				var writer = context.Writer;
				if (value is null)
				{
					writer.WriteNull();
				}
				else
				{
					writer.WriteStartDocument();
					if (value.Id is not null)
					{
						writer.WriteName("_id");
						writer.WriteString(value.Id);
					}
					if (value.Name is not null)
					{
						writer.WriteName("Name");
						writer.WriteString(value.Name);
					}
					if (value.Description is not null)
					{
						writer.WriteName("Description");
						writer.WriteString(value.Description);
					}
					if (value.StartDate != default)
					{
						writer.WriteName("StartDate");
						writer.WriteDateTime(new BsonDateTime(value.StartDate).MillisecondsSinceEpoch);
					}
					if (value.EndDate is not null)
					{
						writer.WriteName("EndDate");
						writer.WriteDateTime(new BsonDateTime(value.EndDate.Value).MillisecondsSinceEpoch);
					}
					if (value.Count != default)
					{
						writer.WriteName("Count");
						writer.WriteInt32(value.Count);
					}
					if (value.IsEnabled)
					{
						writer.WriteName("IsEnabled");
						writer.WriteBoolean(true);
					}
					if (value.Data is not null)
					{
						writer.WriteName("Data");
						// NOTE: This would normally be in a separate serializer that would still need to be determined at runtime
						writer.WriteStartDocument();
						writer.WriteName("StatusCode");
						writer.WriteInt32((int)value.Data.StatusCode);
						if (value.Data.Time != default)
						{
							writer.WriteName("Time");
							writer.WriteDouble(value.Data.Time);
						}
						if (value.Data.Items is not null && value.Data.Items.Length > 0)
						{
							writer.WriteName("Items");
							writer.WriteStartArray();
							for (int i = 0, l = value.Data.Items.Length; i < l; i++)
							{
								writer.WriteString(value.Data.Items[i]);
							}
							writer.WriteEndArray();
						}
						writer.WriteEndDocument();
					}
					writer.WriteEndDocument();
				}
			}

			public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
			{
				throw new NotImplementedException();
			}

			object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
			{
				throw new NotImplementedException();
			}
		}

		private readonly ExampleClass Instance = new()
		{
			Id = "1",
			Name = "Hello World",
			Description = "This is a description of Hello World",
			StartDate = DateTime.Parse("2020-04-08T13:45:15Z"),
			Count = 17,
			IsEnabled = true,
			Data = new NestedObject
			{
				StatusCode = HttpStatusCode.ExpectationFailed,
				Time = 12354542.12,
				Items = new[]
				{
					"Foo",
					"Bar",
					"FooBar",
					"Fizz"
				}
			}
		};

		private BsonSerializationContext NewSerializationContext()
		{
			return BsonSerializationContext.CreateRoot(new BsonDocumentWriter(new BsonDocument()));
		}

		[Benchmark]
		public void MongoDbDriver()
		{
			var context = NewSerializationContext();
			var serializer = BsonSerializer.LookupSerializer<ExampleClass>();
			serializer.Serialize(context, Instance);
		}

		[Benchmark]
		public void CustomSerializer()
		{
			var context = NewSerializationContext();
			var serializer = new ExampleCustomSerializer();
			serializer.Serialize(context, Instance);
		}
	}
}
