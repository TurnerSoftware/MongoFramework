using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Benchmarks.Infrastructure.Mapping
{
	[SimpleJob(RuntimeMoniker.Net50), MemoryDiagnoser]
	public class EntityMappingBenchmark
	{
		public class PersonModel
		{
			public ObjectId Id { get; set; }
			public string Name { get; set; }
			public DateTime DateOfBirth { get; set; }
			public List<AddressModel> Addresses { get; set; }
		}
		public class AddressModel
		{
			public string Street { get; set; }
			public string Suburb { get; set; }
			public string State { get; set; }
			public string Country { get; set; }
		}

		[Benchmark]
		public void ResetOverhead()
		{
			EntityMapping.RemoveAllDefinitions();
			MongoDbDriverHelper.ResetDriver();
		}

		[Benchmark]
		public void ClassMapAutoMap()
		{
			new BsonClassMap<PersonModel>().AutoMap();
			MongoDbDriverHelper.ResetDriver();
		}

		[Benchmark]
		public void EntityMappingRegister()
		{
			EntityMapping.RegisterType(typeof(PersonModel));
			EntityMapping.RemoveAllDefinitions();
			MongoDbDriverHelper.ResetDriver();
		}
	}
}
