using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MongoFramework.Tests
{
	[TestClass]
	public class MongoDbSetTests : TestBase
	{
		public class MongoDbSetValidationModel
		{
			public string Id { get; set; }

			[Required]
			public string RequiredField { get; set; }
		}

		[TestMethod]
		public void InitialiseDbSet()
		{
			var connectionString = TestConfiguration.ConnectionString;
			var databaseName = TestConfiguration.GetDatabaseName();
			AssertExtensions.DoesNotThrow<Exception>(() =>
				new MongoDbSet<MongoDbSetValidationModel>(connectionString, databaseName));
		}

		[TestMethod, ExpectedException(typeof(ValidationException))]
		public void ValidationExceptionOnInvalidModel()
		{
			var database = TestConfiguration.GetDatabase();
			var dbSet = new MongoDbSet<MongoDbSetValidationModel>();
			dbSet.SetConnection(database);

			dbSet.Add(new MongoDbSetValidationModel());
			dbSet.SaveChanges();
		}

		[TestMethod]
		public void SuccessfulInsertAndQueryBack()
		{
			var database = TestConfiguration.GetDatabase();
			var dbSet = new MongoDbSet<MongoDbSetValidationModel>();
			dbSet.SetConnection(database);

			dbSet.Add(new MongoDbSetValidationModel
			{
				RequiredField = "ValueSync"
			});

			Assert.IsFalse(dbSet.Any(m => m.RequiredField == "ValueSync"));
			dbSet.SaveChanges();
			Assert.IsTrue(dbSet.Any(m => m.RequiredField == "ValueSync"));
		}

		[TestMethod]
		public async Task SuccessfulInsertAndQueryBackAsync()
		{
			var database = TestConfiguration.GetDatabase();
			var dbSet = new MongoDbSet<MongoDbSetValidationModel>();
			dbSet.SetConnection(database);

			dbSet.Add(new MongoDbSetValidationModel
			{
				RequiredField = "ValueAsync"
			});

			Assert.IsFalse(dbSet.Any(m => m.RequiredField == "ValueAsync"));
			await dbSet.SaveChangesAsync().ConfigureAwait(false);
			Assert.IsTrue(dbSet.Any(m => m.RequiredField == "ValueAsync"));
		}
	}
}