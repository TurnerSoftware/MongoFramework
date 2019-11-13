using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure.Querying;

namespace MongoFramework.Tests.Infrastructure.Querying
{
	[TestClass]
	public class QueryMappingTests : TestBase
	{
		private class QueryTestModel
		{
			public string Id { get; set; }
			public int SomeNumberField { get; set; }
			public string AnotherStringField { get; set; }
			public NestedQueryTestModel NestedModel { get; set; }
		}

		private class NestedQueryTestModel
		{
			public string Name { get; set; }
			public int Number { get; set; }
		}

		[TestMethod]
		public void EmptyQueryableHasNoStages()
		{
			var queryable = Queryable.AsQueryable(new[] {
				new QueryTestModel()
			});

			var stages = StageBuilder.BuildFromExpression(queryable.Expression);
			Assert.AreEqual(0, stages.Count());
		}

		[TestMethod]
		public void Queryable_Where()
		{
			var queryable = Queryable.AsQueryable(new[] {
				new QueryTestModel()
			}).Where(q => q.Id == "");

			var stages = StageBuilder.BuildFromExpression(queryable.Expression);
			Assert.AreEqual(1, stages.Count());
		}

		[TestMethod]
		public void Queryable_Where_OrderBy()
		{
			var queryable = Queryable.AsQueryable(new[] {
				new QueryTestModel()
			}).Where(q => q.Id == "").OrderBy(q => q.Id);

			var stages = StageBuilder.BuildFromExpression(queryable.Expression);
			Assert.AreEqual(2, stages.Count());
		}

		[TestMethod]
		public void Queryable_Where_OrderByDescending()
		{
			var queryable = Queryable.AsQueryable(new[] {
				new QueryTestModel()
			}).Where(q => q.Id == "").OrderByDescending(q => q.Id);

			var stages = StageBuilder.BuildFromExpression(queryable.Expression);
			Assert.AreEqual(2, stages.Count());
		}

		[TestMethod]
		public void Queryable_Where_OrderBy_Select_Property()
		{
			var queryable = Queryable.AsQueryable(new[] {
				new QueryTestModel()
			}).Where(q => q.Id == "").OrderBy(q => q.Id).Select(q => q.SomeNumberField);

			var stages = StageBuilder.BuildFromExpression(queryable.Expression);
			Assert.AreEqual(3, stages.Count());
		}

		[TestMethod]
		public void Queryable_Where_OrderBy_Select_New()
		{
			var queryable = Queryable.AsQueryable(new[] {
				new QueryTestModel()
			}).Where(q => q.Id == "").OrderBy(q => q.Id).Select(q => new
			{
				MyOwnCustomId = q.Id,
				MyNestedProperty = q.NestedModel.Name
			});

			var stages = StageBuilder.BuildFromExpression(queryable.Expression);
			Assert.AreEqual(3, stages.Count());
		}
	}
}
