using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoFramework.Infrastructure.DefinitionHelpers;

namespace MongoFramework.Benchmarks.Infrastructure.DefinitionHelpers
{
	[SimpleJob(RuntimeMoniker.Net50), MemoryDiagnoser]
	public class UpdateDefinitionHelperBenchmark
	{
		[Benchmark]
		public UpdateDefinition<BsonDocument> Benchmark()
		{
			return UpdateDefinitionHelper.CreateFromDiff<BsonDocument>(
				new BsonDocument(new Dictionary<string, object>
				{
					{"Age", 20},
					{"Name", "Peter"},
					{"FavouriteNumber", 8},
					{"RegisteredDate", new DateTime(2017, 10, 1)},
					{"FavouriteDate", new DateTime(2019, 8, 1)},
					{"SomeBoolean", true},
					{"RelatedIds", new int[] { 1, 3, 5, 7 }},
					{"Status", "Active"},
					{"DescriptionOne", null},
					{"DescriptionTwo", "Hello World"},
					{"ExclusivePropertyToFirst", "Hello World"}
				}),
				new BsonDocument(new Dictionary<string, object>
				{
					{"Age", 21},
					{"Name", "Simon"},
					{"FavouriteNumber", 8},
					{"RegisteredDate", new DateTime(2017, 8, 1)},
					{"FavouriteDate", new DateTime(2019, 8, 1)},
					{"SomeBoolean", false},
					{"RelatedIds", new int[] { 1, 3, 6, 7 }},
					{"MoreNumbers", new int[] { 1, 3, 5, 7, 9 }},
					{"Status", "Active"},
					{"DescriptionOne", "Hello World"},
					{"DescriptionTwo", null},
					{"ExclusivePropertyToSecond", "Hello World"}
				})
			);
		}
	}
}
