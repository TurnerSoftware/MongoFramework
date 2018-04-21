using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Infrastructure.Mapping.Processors;

namespace MongoFramework.Tests.Mapping.Processors.RuntimeType
{
	[TestClass]
	public class RuntimeTypeProcessorTests
	{
		[TestMethod]
		public void MappingAppliesDocumentSerializer()
		{
			var processor = new RuntimeTypeProcessor();
			var classMap = new BsonClassMap<RuntimeTypeMappingModel>();
			classMap.AutoMap();

			var memberSerializers = classMap.DeclaredMemberMaps.ToDictionary(m => m.MemberName, m => m);
			var expectedSerializers = new Dictionary<string, Type>
			{
				{ "KnownCustomCollection", typeof(TypeDiscoveryDocumentSerializer<CustomCollectionModel>) }
			};

			foreach (var expectedSerializer in expectedSerializers)
			{
				Assert.AreNotEqual(expectedSerializer.Value, memberSerializers[expectedSerializer.Key].GetSerializer().GetType());
			}

			processor.ApplyMapping(typeof(RuntimeTypeMappingModel), classMap);

			foreach (var expectedSerializer in expectedSerializers)
			{
				Assert.AreEqual(expectedSerializer.Value, memberSerializers[expectedSerializer.Key].GetSerializer().GetType());
			}
		}

		[TestMethod]
		public void MappingAppliesArraySerializer()
		{
			var processor = new RuntimeTypeProcessor();
			var classMap = new BsonClassMap<RuntimeTypeMappingModel>();
			classMap.AutoMap();

			var memberSerializers = classMap.DeclaredMemberMaps.ToDictionary(m => m.MemberName, m => m);

			var expectedSerializers = new Dictionary<string, Type>
			{
				{ "KnownEnumerableInterface", typeof(TypeDiscoveryArraySerializer<KnownBaseModel, IEnumerable<KnownBaseModel>>) },
				{ "KnownCollectionInterface", typeof(TypeDiscoveryArraySerializer<KnownBaseModel, ICollection<KnownBaseModel>>) },
				{ "KnownListInterface", typeof(TypeDiscoveryArraySerializer<KnownBaseModel, IList<KnownBaseModel>>) },
				{ "KnownListImplementation", typeof(TypeDiscoveryArraySerializer<KnownBaseModel, List<KnownBaseModel>>) }
			};

			foreach (var expectedSerializer in expectedSerializers)
			{
				Assert.AreNotEqual(expectedSerializer.Value, memberSerializers[expectedSerializer.Key].GetSerializer().GetType());
			}

			processor.ApplyMapping(typeof(RuntimeTypeMappingModel), classMap);

			foreach (var expectedSerializer in expectedSerializers)
			{
				Assert.AreEqual(expectedSerializer.Value, memberSerializers[expectedSerializer.Key].GetSerializer().GetType());
			}
		}

		[TestMethod, ExpectedException(typeof(NotSupportedException))]
		public void UnsupportedTypeForArraySerialization()
		{
			var processor = new RuntimeTypeProcessor();
			var classMap = new BsonClassMap<UnsupportedArrayTypeModel>();
			classMap.AutoMap();

			processor.ApplyMapping(typeof(UnsupportedArrayTypeModel), classMap);
		}
	}
}
