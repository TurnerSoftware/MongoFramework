using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MongoFramework.Tests.Infrastructure.Serialization
{
	[TestClass]
	public class DateOnlyTimeOnlyTests : TestBase
	{
		public class DateOnlyModel
		{
			public string Id { get; set; }
			public DateOnly Date { get; set; }
			public DateOnly? NullableDate { get; set; }
		}

		public class TimeOnlyModel
		{
			public string Id { get; set; }
			public TimeOnly Time { get; set; }
			public TimeOnly? NullableTime { get; set; }
		}

		public class CombinedModel
		{
			public string Id { get; set; }
			public DateOnly Date { get; set; }
			public TimeOnly Time { get; set; }
			public DateOnly? NullableDate { get; set; }
			public TimeOnly? NullableTime { get; set; }
		}

		[TestMethod]
		public void DateOnly_InsertAndQueryBack()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<DateOnlyModel>(context);

			var testDate = new DateOnly(2024, 6, 15);
			var entity = new DateOnlyModel
			{
				Date = testDate,
				NullableDate = new DateOnly(2024, 12, 25)
			};

			dbSet.Add(entity);
			context.SaveChanges();

			// Query back
			var result = dbSet.FirstOrDefault(x => x.Id == entity.Id);

			Assert.IsNotNull(result);
			Assert.AreEqual(testDate, result.Date);
			Assert.AreEqual(new DateOnly(2024, 12, 25), result.NullableDate);
		}

		[TestMethod]
		public void DateOnly_NullableWithNullValue()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<DateOnlyModel>(context);

			var testDate = new DateOnly(2024, 1, 1);
			var entity = new DateOnlyModel
			{
				Date = testDate,
				NullableDate = null
			};

			dbSet.Add(entity);
			context.SaveChanges();

			var result = dbSet.FirstOrDefault(x => x.Id == entity.Id);

			Assert.IsNotNull(result);
			Assert.AreEqual(testDate, result.Date);
			Assert.IsNull(result.NullableDate);
		}

		[TestMethod]
		public void DateOnly_QueryWithPredicate()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<DateOnlyModel>(context);

			var targetDate = new DateOnly(2024, 7, 4);
			dbSet.Add(new DateOnlyModel { Date = new DateOnly(2024, 1, 1) });
			dbSet.Add(new DateOnlyModel { Date = targetDate });
			dbSet.Add(new DateOnlyModel { Date = new DateOnly(2024, 12, 31) });

			context.SaveChanges();

			var result = dbSet.FirstOrDefault(x => x.Date == targetDate);

			Assert.IsNotNull(result);
			Assert.AreEqual(targetDate, result.Date);
		}

		[TestMethod]
		public void DateOnly_QueryWithComparison()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<DateOnlyModel>(context);

			dbSet.Add(new DateOnlyModel { Date = new DateOnly(2024, 1, 1) });
			dbSet.Add(new DateOnlyModel { Date = new DateOnly(2024, 6, 15) });
			dbSet.Add(new DateOnlyModel { Date = new DateOnly(2024, 12, 31) });

			context.SaveChanges();

			var midYear = new DateOnly(2024, 6, 1);
			var results = dbSet.Where(x => x.Date > midYear).ToArray();

			Assert.HasCount(2, results);
		}

		[TestMethod]
		public void TimeOnly_InsertAndQueryBack()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<TimeOnlyModel>(context);

			var testTime = new TimeOnly(14, 30, 45);
			var entity = new TimeOnlyModel
			{
				Time = testTime,
				NullableTime = new TimeOnly(23, 59, 59)
			};

			dbSet.Add(entity);
			context.SaveChanges();

			var result = dbSet.FirstOrDefault(x => x.Id == entity.Id);

			Assert.IsNotNull(result);
			Assert.AreEqual(testTime, result.Time);
			Assert.AreEqual(new TimeOnly(23, 59, 59), result.NullableTime);
		}

		[TestMethod]
		public void TimeOnly_NullableWithNullValue()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<TimeOnlyModel>(context);

			var testTime = new TimeOnly(9, 0, 0);
			var entity = new TimeOnlyModel
			{
				Time = testTime,
				NullableTime = null
			};

			dbSet.Add(entity);
			context.SaveChanges();

			var result = dbSet.FirstOrDefault(x => x.Id == entity.Id);

			Assert.IsNotNull(result);
			Assert.AreEqual(testTime, result.Time);
			Assert.IsNull(result.NullableTime);
		}

		[TestMethod]
		public void TimeOnly_QueryWithPredicate()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<TimeOnlyModel>(context);

			var targetTime = new TimeOnly(12, 0, 0);
			dbSet.Add(new TimeOnlyModel { Time = new TimeOnly(8, 0, 0) });
			dbSet.Add(new TimeOnlyModel { Time = targetTime });
			dbSet.Add(new TimeOnlyModel { Time = new TimeOnly(18, 0, 0) });

			context.SaveChanges();

			var result = dbSet.FirstOrDefault(x => x.Time == targetTime);

			Assert.IsNotNull(result);
			Assert.AreEqual(targetTime, result.Time);
		}

		[TestMethod]
		public void TimeOnly_QueryWithComparison()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<TimeOnlyModel>(context);

			dbSet.Add(new TimeOnlyModel { Time = new TimeOnly(8, 0, 0) });
			dbSet.Add(new TimeOnlyModel { Time = new TimeOnly(12, 0, 0) });
			dbSet.Add(new TimeOnlyModel { Time = new TimeOnly(18, 0, 0) });

			context.SaveChanges();

			var noon = new TimeOnly(12, 0, 0);
			var results = dbSet.Where(x => x.Time >= noon).ToArray();

			Assert.HasCount(2, results);
		}

		[TestMethod]
		public void TimeOnly_WithMilliseconds()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<TimeOnlyModel>(context);

			var testTime = new TimeOnly(10, 30, 45, 123);
			var entity = new TimeOnlyModel
			{
				Time = testTime
			};

			dbSet.Add(entity);
			context.SaveChanges();

			var result = dbSet.FirstOrDefault(x => x.Id == entity.Id);

			Assert.IsNotNull(result);
			Assert.AreEqual(testTime.Hour, result.Time.Hour);
			Assert.AreEqual(testTime.Minute, result.Time.Minute);
			Assert.AreEqual(testTime.Second, result.Time.Second);
			Assert.AreEqual(testTime.Millisecond, result.Time.Millisecond);
		}

		[TestMethod]
		public void Combined_DateOnlyAndTimeOnly()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<CombinedModel>(context);

			var testDate = new DateOnly(2024, 6, 15);
			var testTime = new TimeOnly(14, 30, 0);

			var entity = new CombinedModel
			{
				Date = testDate,
				Time = testTime,
				NullableDate = new DateOnly(2025, 1, 1),
				NullableTime = new TimeOnly(9, 0, 0)
			};

			dbSet.Add(entity);
			context.SaveChanges();

			var result = dbSet.FirstOrDefault(x => x.Id == entity.Id);

			Assert.IsNotNull(result);
			Assert.AreEqual(testDate, result.Date);
			Assert.AreEqual(testTime, result.Time);
			Assert.AreEqual(new DateOnly(2025, 1, 1), result.NullableDate);
			Assert.AreEqual(new TimeOnly(9, 0, 0), result.NullableTime);
		}

		[TestMethod]
		public async Task DateOnly_AsyncInsertAndQueryBack()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<DateOnlyModel>(context);

			var testDate = new DateOnly(2024, 3, 14);
			var entity = new DateOnlyModel
			{
				Date = testDate
			};

			dbSet.Add(entity);
			await context.SaveChangesAsync();

			var result = dbSet.FirstOrDefault(x => x.Id == entity.Id);

			Assert.IsNotNull(result);
			Assert.AreEqual(testDate, result.Date);
		}

		[TestMethod]
		public async Task TimeOnly_AsyncInsertAndQueryBack()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<TimeOnlyModel>(context);

			var testTime = new TimeOnly(16, 45, 30);
			var entity = new TimeOnlyModel
			{
				Time = testTime
			};

			dbSet.Add(entity);
			await context.SaveChangesAsync();

			var result = dbSet.FirstOrDefault(x => x.Id == entity.Id);

			Assert.IsNotNull(result);
			Assert.AreEqual(testTime, result.Time);
		}

		[TestMethod]
		public void DateOnly_Update()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<DateOnlyModel>(context);

			var entity = new DateOnlyModel
			{
				Date = new DateOnly(2024, 1, 1)
			};

			dbSet.Add(entity);
			context.SaveChanges();

			// Update
			entity.Date = new DateOnly(2024, 12, 31);
			dbSet.Update(entity);
			context.SaveChanges();

			// Verify
			ResetMongoDb();
			context = new MongoDbContext(connection);
			dbSet = new MongoDbSet<DateOnlyModel>(context);

			var result = dbSet.FirstOrDefault(x => x.Id == entity.Id);
			Assert.AreEqual(new DateOnly(2024, 12, 31), result.Date);
		}

		[TestMethod]
		public void TimeOnly_Update()
		{
			var connection = TestConfiguration.GetConnection();
			var context = new MongoDbContext(connection);
			var dbSet = new MongoDbSet<TimeOnlyModel>(context);

			var entity = new TimeOnlyModel
			{
				Time = new TimeOnly(8, 0, 0)
			};

			dbSet.Add(entity);
			context.SaveChanges();

			// Update
			entity.Time = new TimeOnly(20, 0, 0);
			dbSet.Update(entity);
			context.SaveChanges();

			// Verify
			ResetMongoDb();
			context = new MongoDbContext(connection);
			dbSet = new MongoDbSet<TimeOnlyModel>(context);

			var result = dbSet.FirstOrDefault(x => x.Id == entity.Id);
			Assert.AreEqual(new TimeOnly(20, 0, 0), result.Time);
		}
	}
}
