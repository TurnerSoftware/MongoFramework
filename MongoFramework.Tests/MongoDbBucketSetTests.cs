using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
			var database = TestConfiguration.GetDatabase();
			var dbSet = new MongoDbBucketSet<EntityGroup, SubEntityClass>(new BucketSetOptions
			{
				BucketSize = 100
			});
			dbSet.SetDatabase(database);

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
			var database = TestConfiguration.GetDatabase();
			var dbSet = new MongoDbBucketSet<EntityGroup, SubEntityClass>(new BucketSetOptions
			{
				BucketSize = 2
			});
			dbSet.SetDatabase(database);

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
			var database = TestConfiguration.GetDatabase();
			var dbSet = new MongoDbBucketSet<EntityGroup, SubEntityClass>(new BucketSetOptions
			{
				BucketSize = 2
			});
			dbSet.SetDatabase(database);

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
	}
}
