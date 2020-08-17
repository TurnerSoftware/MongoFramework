using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MongoFramework.Tests
{
	[TestClass]
	public class MongoDbBucketSetTests : TestBase
	{
		public class EntityGroup
		{
			public string Name { get; set; }
			public int Reference { get; set; }
		}

		public class SubEntityClass
		{
			public string Label { get; set; }
			public DateTime Date { get; set; }
		}

		[TestMethod]
		public void InitialiseDbSet()
		{
			AssertExtensions.DoesNotThrow<Exception>(() => new MongoDbBucketSet<EntityGroup, SubEntityClass>(new Mock<IMongoDbContext>().Object, new BucketSetOptions
			{
				BucketSize = 100,
				EntityTimeProperty = "Date"
			}));
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void MustBeSuppliedContext()
		{
			new MongoDbBucketSet<EntityGroup, SubEntityClass>(null, new BucketSetOptions
			{
				BucketSize = 100,
				EntityTimeProperty = "Date"
			});
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void MustBeSuppliedOptions()
		{
			new MongoDbBucketSet<EntityGroup, SubEntityClass>(new Mock<IMongoDbContext>().Object, null);
		}

		[TestMethod]
		public void SuccessfullyInsertAndQueryBackEntityBuckets()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbBucketSet<EntityGroup, SubEntityClass>(context, new BucketSetOptions
			{
				BucketSize = 100,
				EntityTimeProperty = "Date"
			});

			dbSet.Add(new EntityGroup
			{
				Name = "Group1"
			}, new SubEntityClass
			{
				Label = "Entry1",
				Date = new DateTime(2020, 1, 1)
			});

			Assert.IsFalse(dbSet.Any(b => b.Group.Name == "Group1"));
			context.SaveChanges();
			Assert.IsTrue(dbSet.Any(b => b.Group.Name == "Group1" && b.Items.Any(i => i.Label == "Entry1" && i.Date == new DateTime(2020, 1, 1))));
		}

		[TestMethod]
		public async Task SuccessfullyInsertAndQueryBackEntityBucketsAsync()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbBucketSet<EntityGroup, SubEntityClass>(context, new BucketSetOptions
			{
				BucketSize = 100,
				EntityTimeProperty = "Date"
			});

			dbSet.Add(new EntityGroup
			{
				Name = "Group1"
			}, new SubEntityClass
			{
				Label = "Entry1",
				Date = new DateTime(2020, 1, 1)
			});

			Assert.IsFalse(dbSet.Any(b => b.Group.Name == "Group1"));
			await context.SaveChangesAsync();
			Assert.IsTrue(dbSet.Any(b => b.Group.Name == "Group1" && b.Items.Any(i => i.Label == "Entry1" && i.Date == new DateTime(2020, 1, 1))));
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void InvalidBucketSize()
		{
			new MongoDbBucketSet<EntityGroup, SubEntityClass>(new Mock<IMongoDbContext>().Object, new BucketSetOptions
			{
				BucketSize = 0,
				EntityTimeProperty = "Date"
			});
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void InvalidSubEntityTimeProperty_Missing()
		{
			new MongoDbBucketSet<EntityGroup, SubEntityClass>(new Mock<IMongoDbContext>().Object, new BucketSetOptions
			{
				BucketSize = 100,
				EntityTimeProperty = "MissingField"
			});
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void InvalidSubEntityTimeProperty_WrongType()
		{
			new MongoDbBucketSet<EntityGroup, SubEntityClass>(new Mock<IMongoDbContext>().Object, new BucketSetOptions
			{
				BucketSize = 100,
				EntityTimeProperty = "Label"
			});
		}

		[TestMethod]
		public void RemoveBucket()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbBucketSet<EntityGroup, SubEntityClass>(context, new BucketSetOptions
			{
				BucketSize = 2,
				EntityTimeProperty = "Date"
			});

			dbSet.AddRange(new EntityGroup
			{
				Name = "Group1"
			}, new[] {
				new SubEntityClass
				{
					Label = "Entry1",
					Date = new DateTime(2020, 1, 1)
				},
				new SubEntityClass
				{
					Label = "Entry2",
					Date = new DateTime(2020, 1, 2)
				},
				new SubEntityClass
				{
					Label = "Entry2",
					Date = new DateTime(2020, 1, 3)
				}
			});

			Assert.IsFalse(dbSet.Any(b => b.Group.Name == "Group1"));
			context.SaveChanges();
			Assert.IsTrue(dbSet.Any(b => b.Group.Name == "Group1" && b.ItemCount == 2));
			Assert.IsTrue(dbSet.Any(b => b.Group.Name == "Group1" && b.ItemCount == 1));

			dbSet.Remove(new EntityGroup
			{
				Name = "Group1"
			});
			context.SaveChanges();

			Assert.IsFalse(dbSet.Any(b => b.Group.Name == "Group1"));
		}

		[TestMethod]
		public void FillIntoAdditionalBuckets()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbBucketSet<EntityGroup, SubEntityClass>(context, new BucketSetOptions
			{
				BucketSize = 2,
				EntityTimeProperty = "Date"
			});

			dbSet.AddRange(new EntityGroup
			{
				Name = "Group1"
			}, new[] {
				new SubEntityClass
				{
					Label = "Entry1",
					Date = new DateTime(2020, 1, 1)
				},
				new SubEntityClass
				{
					Label = "Entry2",
					Date = new DateTime(2020, 1, 2)
				},
				new SubEntityClass
				{
					Label = "Entry3",
					Date = new DateTime(2020, 1, 3)
				}
			});

			var a = MongoFramework.Infrastructure.Mapping.EntityMapping.GetOrCreateDefinition(typeof(EntityBucket<EntityGroup, SubEntityClass>));
			var cb = MongoDB.Bson.Serialization.BsonClassMap.LookupClassMap(typeof(EntityBucket<EntityGroup, SubEntityClass>));
			Assert.IsFalse(dbSet.Any(b => b.Group.Name == "Group1"));
			context.SaveChanges();
			Assert.IsTrue(dbSet.Any(b => b.Group.Name == "Group1" && b.ItemCount == 2));
			Assert.IsTrue(dbSet.Any(b => b.Group.Name == "Group1" && b.ItemCount == 1));
		}

		[TestMethod]
		public void BackfillIntoExistingBucket()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbBucketSet<EntityGroup, SubEntityClass>(context, new BucketSetOptions
			{
				BucketSize = 2,
				EntityTimeProperty = "Date"
			});

			dbSet.Add(new EntityGroup
			{
				Name = "Group1"
			}, new SubEntityClass
			{
				Label = "Entry1",
				Date = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
			});
			context.SaveChanges();
			Assert.IsTrue(dbSet.Any(b => b.Group.Name == "Group1" && b.ItemCount == 1));

			dbSet.AddRange(new EntityGroup
			{
				Name = "Group1"
			}, new[] {
				new SubEntityClass
				{
					Label = "Entry2",
					Date = new DateTime(2020, 1, 2, 0, 0, 0, DateTimeKind.Utc)
				},
				new SubEntityClass
				{
					Label = "Entry3",
					Date = new DateTime(2020, 1, 3, 0, 0, 0, DateTimeKind.Utc)
				}
			});
			context.SaveChanges();

			var buckets = dbSet.Where(b => b.Group.Name == "Group1").ToArray();
			Assert.AreEqual(2, buckets.Count());

			var backfilledBucket = buckets[0];
			Assert.AreEqual(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), backfilledBucket.Min);
			Assert.AreEqual(new DateTime(2020, 1, 2, 0, 0, 0, DateTimeKind.Utc), backfilledBucket.Max);
			Assert.AreEqual(2, backfilledBucket.ItemCount);
			Assert.AreEqual("Entry1", backfilledBucket.Items[0].Label);
			Assert.AreEqual("Entry2", backfilledBucket.Items[1].Label);

			var additionalBucket = buckets[1];
			Assert.AreEqual(new DateTime(2020, 1, 3, 0, 0, 0, DateTimeKind.Utc), additionalBucket.Min);
			Assert.AreEqual(new DateTime(2020, 1, 3, 0, 0, 0, DateTimeKind.Utc), additionalBucket.Max);
			Assert.AreEqual(1, additionalBucket.ItemCount);
			Assert.AreEqual("Entry3", additionalBucket.Items[0].Label);
		}

		[TestMethod]
		public void ContinuousSubEntityAccessAcrossBuckets()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbBucketSet<EntityGroup, SubEntityClass>(context, new BucketSetOptions
			{
				BucketSize = 2,
				EntityTimeProperty = "Date"
			});
			
			dbSet.AddRange(new EntityGroup
			{
				Name = "Group1"
			}, new[] {
				new SubEntityClass
				{
					Label = "Entry1",
					Date = new DateTime(2020, 1, 1)
				},
				new SubEntityClass
				{
					Label = "Entry2",
					Date = new DateTime(2020, 1, 2)
				},
				new SubEntityClass
				{
					Label = "Entry3",
					Date = new DateTime(2020, 1, 3)
				},
				new SubEntityClass
				{
					Label = "Entry4",
					Date = new DateTime(2020, 1, 4)
				},
				new SubEntityClass
				{
					Label = "Entry5",
					Date = new DateTime(2020, 1, 5)
				}
			});

			context.SaveChanges();

			Assert.AreEqual(3, dbSet.Count());

			var results = dbSet.WithGroup(new EntityGroup
			{
				Name = "Group1"
			}).ToArray();

			Assert.AreEqual(5, results.Length);
			Assert.AreEqual("Entry1", results[0].Label);
			Assert.AreEqual("Entry2", results[1].Label);
			Assert.AreEqual("Entry3", results[2].Label);
			Assert.AreEqual("Entry4", results[3].Label);
			Assert.AreEqual("Entry5", results[4].Label);
		}

		[TestMethod]
		public void DistinctGroups()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbBucketSet<EntityGroup, SubEntityClass>(context, new BucketSetOptions
			{
				BucketSize = 2,
				EntityTimeProperty = "Date"
			});

			dbSet.AddRange(new EntityGroup
			{
				Name = "Group1"
			}, new[] {
				new SubEntityClass
				{
					Label = "Entry1",
					Date = new DateTime(2020, 1, 1)
				},
				new SubEntityClass
				{
					Label = "Entry2",
					Date = new DateTime(2020, 1, 2)
				},
				new SubEntityClass
				{
					Label = "Entry3",
					Date = new DateTime(2020, 1, 3)
				}
			});
			dbSet.AddRange(new EntityGroup
			{
				Name = "Group2"
			}, new[] {
				new SubEntityClass
				{
					Label = "Entry1",
					Date = new DateTime(2020, 1, 1)
				},
				new SubEntityClass
				{
					Label = "Entry2",
					Date = new DateTime(2020, 1, 2)
				},
				new SubEntityClass
				{
					Label = "Entry3",
					Date = new DateTime(2020, 1, 3)
				}
			});

			context.SaveChanges();

			Assert.AreEqual(4, dbSet.Count());

			var results = dbSet.Groups().OrderBy(g => g.Name).ToArray();

			Assert.AreEqual(2, results.Length);
			Assert.AreEqual("Group1", results[0].Name);
			Assert.AreEqual("Group2", results[1].Name);
		}

		[TestMethod]
		public void IterateQueryable()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbBucketSet<EntityGroup, SubEntityClass>(context, new BucketSetOptions
			{
				BucketSize = 2,
				EntityTimeProperty = "Date"
			});

			dbSet.Add(new EntityGroup
			{
				Name = "Group1"
			}, new SubEntityClass
			{
				Label = "Entry1"
			});
			dbSet.Add(new EntityGroup
			{
				Name = "Group2"
			}, new SubEntityClass
			{
				Label = "Entry1"
			});
			dbSet.Add(new EntityGroup
			{
				Name = "Group3"
			}, new SubEntityClass
			{
				Label = "Entry1"
			});

			foreach (var bucket in dbSet)
			{
				Assert.AreEqual("Entry", bucket.Items[0].Label);
			}
		}

		[TestMethod]
		public void InvalidAddArguments()
		{
			var dbSet = new MongoDbBucketSet<EntityGroup, SubEntityClass>(new Mock<IMongoDbContext>().Object, new BucketSetOptions
			{
				BucketSize = 2,
				EntityTimeProperty = "Date"
			});

			Assert.ThrowsException<ArgumentNullException>(() => dbSet.Add(null, new SubEntityClass()));
			Assert.ThrowsException<ArgumentNullException>(() => dbSet.AddRange(null, null));
			Assert.ThrowsException<ArgumentNullException>(() => dbSet.AddRange(new EntityGroup(), null));
		}

		[TestMethod]
		public void InvalidRemoveArguments()
		{
			var dbSet = new MongoDbBucketSet<EntityGroup, SubEntityClass>(new Mock<IMongoDbContext>().Object, new BucketSetOptions
			{
				BucketSize = 2,
				EntityTimeProperty = "Date"
			});

			Assert.ThrowsException<ArgumentNullException>(() => dbSet.Remove(null));
		}

		[TestMethod]
		public void WithGroupOnEmptyBucket()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbBucketSet<EntityGroup, SubEntityClass>(context, new BucketSetOptions
			{
				BucketSize = 2,
				EntityTimeProperty = "Date"
			});

			var result = dbSet.WithGroup(new EntityGroup
			{
				Name = "Group1",
				Reference = 123
			}).Count();

			Assert.AreEqual(0, result);
		}
	}
}
