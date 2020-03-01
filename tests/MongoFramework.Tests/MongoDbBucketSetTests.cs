using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
		}

		[TestMethod]
		public void InitialiseDbSet()
		{
			AssertExtensions.DoesNotThrow<Exception>(() => new MongoDbBucketSet<EntityGroup, SubEntityClass>(new BucketSetOptions
			{
				BucketSize = 100
			}));
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void MustBeSuppliedOptions()
		{
			new MongoDbBucketSet<EntityGroup, SubEntityClass>(null);
		}

		[TestMethod]
		public void SuccessfullyInsertAndQueryBackEntityBuckets()
		{
			var dbSet = new MongoDbBucketSet<EntityGroup, SubEntityClass>(new BucketSetOptions
			{
				BucketSize = 100
			});
			dbSet.SetConnection(TestConfiguration.GetConnection());

			dbSet.Add(new EntityGroup
			{
				Name = "Group1"
			}, new SubEntityClass
			{
				Label = "Entry1"
			});

			Assert.IsFalse(dbSet.Any(b => b.Group.Name == "Group1"));
			dbSet.SaveChanges();
			Assert.IsTrue(dbSet.Any(b => b.Group.Name == "Group1" && b.Items.Any(i => i.Label == "Entry1")));
		}

		[TestMethod]
		public async Task SuccessfullyInsertAndQueryBackEntityBucketsAsync()
		{
			var dbSet = new MongoDbBucketSet<EntityGroup, SubEntityClass>(new BucketSetOptions
			{
				BucketSize = 100
			});
			dbSet.SetConnection(TestConfiguration.GetConnection());

			dbSet.Add(new EntityGroup
			{
				Name = "Group1"
			}, new SubEntityClass
			{
				Label = "Entry1"
			});

			Assert.IsFalse(dbSet.Any(b => b.Group.Name == "Group1"));
			await dbSet.SaveChangesAsync();
			Assert.IsTrue(dbSet.Any(b => b.Group.Name == "Group1" && b.Items.Any(i => i.Label == "Entry1")));
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void InvalidBucketSize()
		{
			new MongoDbBucketSet<EntityGroup, SubEntityClass>(new BucketSetOptions
			{
				BucketSize = 0
			});
		}

		[TestMethod]
		public void FillIntoAdditionalBuckets()
		{
			var dbSet = new MongoDbBucketSet<EntityGroup, SubEntityClass>(new BucketSetOptions
			{
				BucketSize = 2
			});
			dbSet.SetConnection(TestConfiguration.GetConnection());

			dbSet.AddRange(new EntityGroup
			{
				Name = "Group1"
			}, new[] {
				new SubEntityClass
				{
					Label = "Entry1"
				},
				new SubEntityClass
				{
					Label = "Entry2"
				},
				new SubEntityClass
				{
					Label = "Entry2"
				}
			});

			Assert.IsFalse(dbSet.Any(b => b.Group.Name == "Group1"));
			dbSet.SaveChanges();
			Assert.IsTrue(dbSet.Any(b => b.Group.Name == "Group1" && b.ItemCount == 2));
			Assert.IsTrue(dbSet.Any(b => b.Group.Name == "Group1" && b.ItemCount == 1));
		}

		[TestMethod]
		public void BackfillIntoExistingBucket()
		{
			var dbSet = new MongoDbBucketSet<EntityGroup, SubEntityClass>(new BucketSetOptions
			{
				BucketSize = 2
			});
			dbSet.SetConnection(TestConfiguration.GetConnection());

			dbSet.Add(new EntityGroup
			{
				Name = "Group1"
			}, new SubEntityClass
			{
				Label = "Entry1"
			});
			dbSet.SaveChanges();
			Assert.IsTrue(dbSet.Any(b => b.Group.Name == "Group1" && b.ItemCount == 1));

			dbSet.AddRange(new EntityGroup
			{
				Name = "Group1"
			}, new[] {
				new SubEntityClass
				{
					Label = "Entry2"
				},
				new SubEntityClass
				{
					Label = "Entry3"
				}
			});
			dbSet.SaveChanges();

			var buckets = dbSet.Where(b => b.Group.Name == "Group1").ToArray();
			Assert.AreEqual(2, buckets.Count());
			var backfilledBucket = buckets.FirstOrDefault();
			Assert.AreEqual(2, backfilledBucket.ItemCount);
			Assert.AreEqual("Entry1", backfilledBucket.Items[0].Label);
			Assert.AreEqual("Entry2", backfilledBucket.Items[1].Label);

			var additionalBucket = buckets.LastOrDefault();
			Assert.AreEqual(1, additionalBucket.ItemCount);
			Assert.AreEqual("Entry3", additionalBucket.Items[0].Label);
		}

		[TestMethod]
		public void ContinuousSubEntityAccessAcrossBuckets()
		{
			var dbSet = new MongoDbBucketSet<EntityGroup, SubEntityClass>(new BucketSetOptions
			{
				BucketSize = 2
			});
			dbSet.SetConnection(TestConfiguration.GetConnection());
			
			dbSet.AddRange(new EntityGroup
			{
				Name = "Group1"
			}, new[] {
				new SubEntityClass
				{
					Label = "Entry1"
				},
				new SubEntityClass
				{
					Label = "Entry2"
				},
				new SubEntityClass
				{
					Label = "Entry3"
				},
				new SubEntityClass
				{
					Label = "Entry4"
				},
				new SubEntityClass
				{
					Label = "Entry5"
				}
			});
			dbSet.SaveChanges();

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
			var dbSet = new MongoDbBucketSet<EntityGroup, SubEntityClass>(new BucketSetOptions
			{
				BucketSize = 2
			});
			dbSet.SetConnection(TestConfiguration.GetConnection());

			dbSet.AddRange(new EntityGroup
			{
				Name = "Group1"
			}, new[] {
				new SubEntityClass
				{
					Label = "Entry1"
				},
				new SubEntityClass
				{
					Label = "Entry2"
				},
				new SubEntityClass
				{
					Label = "Entry3"
				}
			});
			dbSet.AddRange(new EntityGroup
			{
				Name = "Group2"
			}, new[] {
				new SubEntityClass
				{
					Label = "Entry1"
				},
				new SubEntityClass
				{
					Label = "Entry2"
				},
				new SubEntityClass
				{
					Label = "Entry3"
				}
			});
			dbSet.SaveChanges();

			Assert.AreEqual(4, dbSet.Count());

			var results = dbSet.Groups().OrderBy(g => g.Name).ToArray();

			Assert.AreEqual(2, results.Length);
			Assert.AreEqual("Group1", results[0].Name);
			Assert.AreEqual("Group2", results[1].Name);
		}

		[TestMethod]
		public void IterateQueryable()
		{
			var dbSet = new MongoDbBucketSet<EntityGroup, SubEntityClass>(new BucketSetOptions
			{
				BucketSize = 2
			});
			dbSet.SetConnection(TestConfiguration.GetConnection());

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
			var dbSet = new MongoDbBucketSet<EntityGroup, SubEntityClass>(new BucketSetOptions
			{
				BucketSize = 2
			});

			Assert.ThrowsException<ArgumentNullException>(() => dbSet.Add(null, new SubEntityClass()));
			Assert.ThrowsException<ArgumentNullException>(() => dbSet.AddRange(null, null));
			Assert.ThrowsException<ArgumentNullException>(() => dbSet.AddRange(new EntityGroup(), null));
		}

		[TestMethod]
		public void ValueTypeSubEntity()
		{
			var dbSet = new MongoDbBucketSet<EntityGroup, int>(new BucketSetOptions
			{
				BucketSize = 2
			});
			dbSet.SetConnection(TestConfiguration.GetConnection());

			dbSet.AddRange(new EntityGroup
			{
				Name = "Group1"
			}, new[] { 2, 4, 6, 8, 10 });
			dbSet.SaveChanges();

			Assert.AreEqual(3, dbSet.Count());

			var results = dbSet.WithGroup(new EntityGroup
			{
				Name = "Group1"
			}).ToArray();

			Assert.AreEqual(5, results.Length);
			Assert.AreEqual(2, results[0]);
			Assert.AreEqual(4, results[1]);
			Assert.AreEqual(6, results[2]);
			Assert.AreEqual(8, results[3]);
			Assert.AreEqual(10, results[4]);
		}

		[TestMethod]
		public void WithGroupOnEmptyBucket()
		{
			var dbSet = new MongoDbBucketSet<EntityGroup, SubEntityClass>(new BucketSetOptions
			{
				BucketSize = 2
			});
			dbSet.SetConnection(TestConfiguration.GetConnection());

			var result = dbSet.WithGroup(new EntityGroup
			{
				Name = "Group1",
				Reference = 123
			}).Count();

			Assert.AreEqual(0, result);
		}
	}
}
