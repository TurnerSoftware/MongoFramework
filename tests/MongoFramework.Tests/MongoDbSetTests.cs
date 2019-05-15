using Microsoft.VisualStudio.TestTools.UnitTesting;
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

		[TestMethod, ExpectedException(typeof(ValidationException))]
		public void ValidationExceptionOnInvalidModel()
		{
			var dbSet = new MongoDbSet<MongoDbSetValidationModel>();
			dbSet.SetConnection(TestConfiguration.GetConnection());

			dbSet.Add(new MongoDbSetValidationModel());
			dbSet.SaveChanges();
		}

		[TestMethod]
		public void SuccessfulInsertAndQueryBack()
		{
			var dbSet = new MongoDbSet<MongoDbSetValidationModel>();
			dbSet.SetConnection(TestConfiguration.GetConnection());

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
			var dbSet = new MongoDbSet<MongoDbSetValidationModel>();
			dbSet.SetConnection(TestConfiguration.GetConnection());

			dbSet.Add(new MongoDbSetValidationModel
			{
				RequiredField = "ValueAsync"
			});

			Assert.IsFalse(dbSet.Any(m => m.RequiredField == "ValueAsync"));
			await dbSet.SaveChangesAsync().ConfigureAwait(false);
			Assert.IsTrue(dbSet.Any(m => m.RequiredField == "ValueAsync"));
		}

		[TestMethod]
		public void SuccessfullyUpdateEntity()
		{
			var dbSet = new MongoDbSet<MongoDbSetValidationModel>();
			dbSet.SetConnection(TestConfiguration.GetConnection());

			var entity = new MongoDbSetValidationModel
			{
				RequiredField = "SuccessfullyUpdateEntity"
			};

			dbSet.Add(entity);
			dbSet.SaveChanges();

			dbSet.SetConnection(TestConfiguration.GetConnection());

			entity.RequiredField = "SuccessfullyUpdateEntity-Updated";
			dbSet.Update(entity);

			Assert.IsFalse(dbSet.Any(m => m.RequiredField == "SuccessfullyUpdateEntity-Updated"));
			dbSet.SaveChanges();
			Assert.IsTrue(dbSet.Any(m => m.RequiredField == "SuccessfullyUpdateEntity-Updated"));
		}

		[TestMethod]
		public void SuccessfullyUpdateRange()
		{
			var dbSet = new MongoDbSet<MongoDbSetValidationModel>();
			dbSet.SetConnection(TestConfiguration.GetConnection());

			var entities = new[] {
				new MongoDbSetValidationModel
				{
					RequiredField = "SuccessfullyUpdateRange.1"
				},
				new MongoDbSetValidationModel
				{
					RequiredField = "SuccessfullyUpdateRange.2"
				}
			};

			dbSet.AddRange(entities);
			dbSet.SaveChanges();

			dbSet.SetConnection(TestConfiguration.GetConnection());

			entities[0].RequiredField = "SuccessfullyUpdateRange.1-Updated";
			entities[1].RequiredField = "SuccessfullyUpdateRange.2-Updated";
			dbSet.UpdateRange(entities);

			Assert.IsFalse(dbSet.Any(m => m.RequiredField == "SuccessfullyUpdateRange.1-Updated" || m.RequiredField == "SuccessfullyUpdateRange.2-Updated"));
			dbSet.SaveChanges();
			Assert.IsTrue(dbSet.Any(m => m.RequiredField == "SuccessfullyUpdateRange.1-Updated"));
			Assert.IsTrue(dbSet.Any(m => m.RequiredField == "SuccessfullyUpdateRange.2-Updated"));
		}

		[TestMethod]
		public void SuccessfullyRemoveEntity()
		{
			var dbSet = new MongoDbSet<MongoDbSetValidationModel>();
			dbSet.SetConnection(TestConfiguration.GetConnection());

			var entity = new MongoDbSetValidationModel
			{
				RequiredField = "SuccessfullyRemoveEntity"
			};

			dbSet.Add(entity);
			dbSet.SaveChanges();

			dbSet.SetConnection(TestConfiguration.GetConnection());

			dbSet.Remove(entity);

			Assert.IsTrue(dbSet.Any(m => m.RequiredField == "SuccessfullyRemoveEntity"));
			dbSet.SaveChanges();
			Assert.IsFalse(dbSet.Any(m => m.RequiredField == "SuccessfullyRemoveEntity"));
		}

		[TestMethod]
		public void SuccessfullyRemoveRange()
		{
			var dbSet = new MongoDbSet<MongoDbSetValidationModel>();
			dbSet.SetConnection(TestConfiguration.GetConnection());

			var entities = new[] {
				new MongoDbSetValidationModel
				{
					RequiredField = "SuccessfullyRemoveRange.1"
				},
				new MongoDbSetValidationModel
				{
					RequiredField = "SuccessfullyRemoveRange.2"
				}
			};

			dbSet.AddRange(entities);
			dbSet.SaveChanges();

			dbSet.SetConnection(TestConfiguration.GetConnection());

			dbSet.RemoveRange(entities);

			Assert.IsTrue(dbSet.Any(m => m.RequiredField == "SuccessfullyRemoveRange.1"));
			Assert.IsTrue(dbSet.Any(m => m.RequiredField == "SuccessfullyRemoveRange.2"));
			dbSet.SaveChanges();
			Assert.IsFalse(dbSet.Any(m => m.RequiredField == "SuccessfullyRemoveRange.1"));
			Assert.IsFalse(dbSet.Any(m => m.RequiredField == "SuccessfullyRemoveRange.2"));
		}
		
		[TestMethod]
		public void SuccessfullyRemoveEntityById()
		{
			var dbSet = new MongoDbSet<MongoDbSetValidationModel>();
			dbSet.SetConnection(TestConfiguration.GetConnection());

			var entity = new MongoDbSetValidationModel
			{
				RequiredField = "SuccessfullyRemoveEntityById"
			};

			dbSet.Add(entity);
			dbSet.SaveChanges();

			dbSet.SetConnection(TestConfiguration.GetConnection());

			dbSet.RemoveById(entity.Id);

			Assert.IsTrue(dbSet.Any(m => m.RequiredField == "SuccessfullyRemoveEntityById"));
			dbSet.SaveChanges();
			Assert.IsFalse(dbSet.Any(m => m.RequiredField == "SuccessfullyRemoveEntityById"));
		}
	}
}