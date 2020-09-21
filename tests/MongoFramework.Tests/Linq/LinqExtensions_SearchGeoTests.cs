using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver.GeoJsonObjectModel;
using MongoFramework.Attributes;
using MongoFramework.Linq;

namespace MongoFramework.Tests.Linq
{
	[TestClass]
	public class LinqExtensions_SearchGeoTests : TestBase
	{
		public class SearchGeoModel
		{
			public string Id { get; set; }
			public string Description { get; set; }
			[Index(IndexType.Geo2dSphere)]
			public GeoJsonPoint<GeoJson2DGeographicCoordinates> PrimaryCoordinates { get; set; }
			[Index(IndexType.Geo2dSphere)]
			public GeoJsonPoint<GeoJson2DGeographicCoordinates> SecondaryCoordinates { get; set; }

			[ExtraElements]
			public IDictionary<string, object> ExtraElements { get; set; }
			public double CustomDistanceField { get; set; }
		}

		[TestMethod]
		public void SearchGeoNear()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<SearchGeoModel>(context);

			dbSet.AddRange(new SearchGeoModel[]
			{
				new SearchGeoModel { Description = "New York", PrimaryCoordinates = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
					new GeoJson2DGeographicCoordinates(-74.005974, 40.712776)
				) },
				new SearchGeoModel { Description = "Adelaide", PrimaryCoordinates = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
					new GeoJson2DGeographicCoordinates(138.600739, -34.928497)
				) },
				new SearchGeoModel { Description = "Perth", PrimaryCoordinates = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
					new GeoJson2DGeographicCoordinates(115.860458, -31.950527)
				) },
				new SearchGeoModel { Description = "Hobart", PrimaryCoordinates = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
					new GeoJson2DGeographicCoordinates(147.327194, -42.882137)
				) }
			});
			context.SaveChanges();

			var results = dbSet.SearchGeoNear(e => e.PrimaryCoordinates, new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
				new GeoJson2DGeographicCoordinates(138, -30)
			)).ToArray();

			Assert.AreEqual(4, results.Count());
			Assert.AreEqual("Adelaide", results[0].Description);
			Assert.AreEqual("New York", results[3].Description);

			Assert.IsTrue(results[0].ExtraElements.ContainsKey("Distance"));
		}

		[TestMethod]
		public void SearchGeoNearWithCustomDistanceField()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<SearchGeoModel>(context);

			dbSet.AddRange(new SearchGeoModel[]
			{
				new SearchGeoModel { Description = "New York", PrimaryCoordinates = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
					new GeoJson2DGeographicCoordinates(-74.005974, 40.712776)
				) },
				new SearchGeoModel { Description = "Adelaide", PrimaryCoordinates = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
					new GeoJson2DGeographicCoordinates(138.600739, -34.928497)
				) }
			});
			context.SaveChanges();

			var results = dbSet.SearchGeoNear(e => e.PrimaryCoordinates, new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
				new GeoJson2DGeographicCoordinates(138, -30)
			), distanceResultField: e => e.CustomDistanceField).ToArray();

			Assert.AreNotEqual(0, results[0].CustomDistanceField);
			Assert.AreNotEqual(0, results[1].CustomDistanceField);
			Assert.IsTrue(results[0].CustomDistanceField < results[1].CustomDistanceField);

			Assert.IsNull(results[0].ExtraElements);
		}

		[TestMethod]
		public void SearchGeoNearWithMinMaxDistances()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<SearchGeoModel>(context);

			dbSet.AddRange(new SearchGeoModel[]
			{
				new SearchGeoModel { Description = "New York", PrimaryCoordinates = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
					new GeoJson2DGeographicCoordinates(-74.005974, 40.712776)
				) },
				new SearchGeoModel { Description = "Adelaide", PrimaryCoordinates = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
					new GeoJson2DGeographicCoordinates(138.600739, -34.928497)
				) },
				new SearchGeoModel { Description = "Perth", PrimaryCoordinates = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
					new GeoJson2DGeographicCoordinates(115.860458, -31.950527)
				) },
				new SearchGeoModel { Description = "Hobart", PrimaryCoordinates = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
					new GeoJson2DGeographicCoordinates(147.327194, -42.882137)
				) }
			});
			context.SaveChanges();

			SearchGeoModel[] GetResults(double? maxDistance = null, double? minDistance = null)
			{
				return dbSet.SearchGeoNear(e => e.PrimaryCoordinates, new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
					new GeoJson2DGeographicCoordinates(138, -30)
				), distanceResultField: e => e.CustomDistanceField, maxDistance: maxDistance, minDistance: minDistance).ToArray();
			}

			var results = GetResults(maxDistance: 3000000);
			Assert.AreEqual(3, results.Count());
			Assert.IsTrue(results.Max(e => e.CustomDistanceField) < 3000000);

			results = GetResults(maxDistance: 600000);
			Assert.AreEqual(1, results.Count());
			Assert.IsTrue(results.Max(e => e.CustomDistanceField) < 600000);

			results = GetResults(maxDistance: 17000000);
			Assert.AreEqual(4, results.Count());

			results = GetResults(minDistance: 600000);
			Assert.AreEqual(3, results.Count());
			Assert.IsTrue(results.Min(e => e.CustomDistanceField) > 600000);

			results = GetResults(maxDistance: 3000000, minDistance: 600000);
			Assert.AreEqual(2, results.Count());
			Assert.IsTrue(results.Max(e => e.CustomDistanceField) < 3000000);
			Assert.IsTrue(results.Min(e => e.CustomDistanceField) > 600000);
		}

		[TestMethod]
		public void SearchGeoIntersects()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<SearchGeoModel>(context);

			dbSet.AddRange(new SearchGeoModel[]
			{
				new SearchGeoModel { Description = "New York", PrimaryCoordinates = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
					new GeoJson2DGeographicCoordinates(-74.005974, 40.712776)
				) },
				new SearchGeoModel { Description = "Adelaide", PrimaryCoordinates = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
					new GeoJson2DGeographicCoordinates(138.600739, -34.928497)
				) },
				new SearchGeoModel { Description = "Sydney", PrimaryCoordinates = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
					new GeoJson2DGeographicCoordinates(151.209290, -33.868820)
				) },
				new SearchGeoModel { Description = "Melbourne", PrimaryCoordinates = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
					new GeoJson2DGeographicCoordinates(144.963058, -37.813629)
				) },
				new SearchGeoModel { Description = "Darwin", PrimaryCoordinates = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
					new GeoJson2DGeographicCoordinates(-95.582413, 37.095142)
				) },
				new SearchGeoModel { Description = "Brisbane", PrimaryCoordinates = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
					new GeoJson2DGeographicCoordinates(153.025131, -27.469770)
				) }
			});
			context.SaveChanges();

			var results = dbSet.SearchGeoIntersecting(e => e.PrimaryCoordinates, new GeoJsonPolygon<GeoJson2DGeographicCoordinates>(
				new GeoJsonPolygonCoordinates<GeoJson2DGeographicCoordinates>(
					new GeoJsonLinearRingCoordinates<GeoJson2DGeographicCoordinates>(
						new[]
						{
							new GeoJson2DGeographicCoordinates(115.860458, -31.950527), //Perth
							new GeoJson2DGeographicCoordinates(147.327194, -42.882137), //Hobart
							new GeoJson2DGeographicCoordinates(153.399994, -28.016666), //Gold Coast

							new GeoJson2DGeographicCoordinates(115.860458, -31.950527) //Wrap back to first point
						}
					)
				)
			)).ToArray();

			Assert.AreEqual(3, results.Count());
			Assert.IsTrue(results.Any(e => e.Description == "Adelaide"));
			Assert.IsTrue(results.Any(e => e.Description == "Melbourne"));
			Assert.IsTrue(results.Any(e => e.Description == "Sydney"));
		}
	}
}
