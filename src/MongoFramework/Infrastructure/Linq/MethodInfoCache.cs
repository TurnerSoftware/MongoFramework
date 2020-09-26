using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MongoFramework.Infrastructure.Linq
{
	internal static class MethodInfoCache
	{
		private static MethodInfo GetMethodInfo<TResult>(Func<IAsyncEnumerable<object>, CancellationToken, ValueTask<TResult>> methodDelegate) => methodDelegate.Method.GetGenericMethodDefinition();
		private static MethodInfo GetMethodInfo<TResult>(Func<IEnumerable<object>, TResult> methodDelegate) => methodDelegate.Method.GetGenericMethodDefinition();
		private static MethodInfo GetMethodInfo<TResult>(Func<IQueryable<object>, TResult> methodDelegate) => methodDelegate.Method.GetGenericMethodDefinition();
		private static MethodInfo GetMethodInfo_WithParameter<TParam>(Func<IQueryable<object>, TParam, object> methodDelegate) => methodDelegate.Method.GetGenericMethodDefinition();
		private static MethodInfo GetMethodInfo_Passthrough_NonGeneric<TSource>(Func<IQueryable<TSource>, TSource> methodDelegate) => methodDelegate.Method;
		private static MethodInfo GetMethodInfo_WithParameter<TParam, TResult>(Func<IQueryable<object>, TParam, TResult> methodDelegate) => methodDelegate.Method.GetGenericMethodDefinition();

		public static class Queryable
		{
			public static readonly MethodInfo First_1 = GetMethodInfo(System.Linq.Queryable.First);
			public static readonly MethodInfo First_2 = GetMethodInfo_WithParameter<Expression<Func<object, bool>>>(System.Linq.Queryable.First);
			public static readonly MethodInfo FirstOrDefault_1 = GetMethodInfo(System.Linq.Queryable.FirstOrDefault);
			public static readonly MethodInfo FirstOrDefault_2 = GetMethodInfo_WithParameter<Expression<Func<object, bool>>>(System.Linq.Queryable.FirstOrDefault);

			public static readonly MethodInfo Single_1 = GetMethodInfo(System.Linq.Queryable.Single);
			public static readonly MethodInfo Single_2 = GetMethodInfo_WithParameter<Expression<Func<object, bool>>>(System.Linq.Queryable.Single);
			public static readonly MethodInfo SingleOrDefault_1 = GetMethodInfo(System.Linq.Queryable.SingleOrDefault);
			public static readonly MethodInfo SingleOrDefault_2 = GetMethodInfo_WithParameter<Expression<Func<object, bool>>>(System.Linq.Queryable.SingleOrDefault);

			public static readonly MethodInfo Count_1 = GetMethodInfo(System.Linq.Queryable.Count);
			public static readonly MethodInfo Count_2 = GetMethodInfo_WithParameter<Expression<Func<object, bool>>, int>(System.Linq.Queryable.Count);

			public static readonly MethodInfo Any_1 = GetMethodInfo(System.Linq.Queryable.Any);
			public static readonly MethodInfo Any_2 = GetMethodInfo_WithParameter<Expression<Func<object, bool>>, bool>(System.Linq.Queryable.Any);

			public static readonly MethodInfo All_1 = GetMethodInfo_WithParameter<Expression<Func<object, bool>>, bool>(System.Linq.Queryable.All);

			public static readonly MethodInfo Max_1 = GetMethodInfo(System.Linq.Queryable.Max);
			public static readonly MethodInfo Max_2 = GetMethodInfo_WithParameter<Expression<Func<object, object>>, object>(System.Linq.Queryable.Max);

			public static readonly MethodInfo Min_1 = GetMethodInfo(System.Linq.Queryable.Min);
			public static readonly MethodInfo Min_2 = GetMethodInfo_WithParameter<Expression<Func<object, object>>, object>(System.Linq.Queryable.Min);

			public static readonly MethodInfo Sum_Int32_1 = GetMethodInfo_Passthrough_NonGeneric<int>(System.Linq.Queryable.Sum);
			public static readonly MethodInfo Sum_NullableInt32_1 = GetMethodInfo_Passthrough_NonGeneric<int?>(System.Linq.Queryable.Sum);
			public static readonly MethodInfo Sum_Decimal_1 = GetMethodInfo_Passthrough_NonGeneric<decimal>(System.Linq.Queryable.Sum);
			public static readonly MethodInfo Sum_NullableDecimal_1 = GetMethodInfo_Passthrough_NonGeneric<decimal?>(System.Linq.Queryable.Sum);
			public static readonly MethodInfo Sum_Double_1 = GetMethodInfo_Passthrough_NonGeneric<double>(System.Linq.Queryable.Sum);
			public static readonly MethodInfo Sum_NullableDouble_1 = GetMethodInfo_Passthrough_NonGeneric<double?>(System.Linq.Queryable.Sum);
			public static readonly MethodInfo Sum_Long_1 = GetMethodInfo_Passthrough_NonGeneric<long>(System.Linq.Queryable.Sum);
			public static readonly MethodInfo Sum_NullableLong_1 = GetMethodInfo_Passthrough_NonGeneric<long?>(System.Linq.Queryable.Sum);
			public static readonly MethodInfo Sum_Float_1 = GetMethodInfo_Passthrough_NonGeneric<float>(System.Linq.Queryable.Sum);
			public static readonly MethodInfo Sum_NullableFloat_1 = GetMethodInfo_Passthrough_NonGeneric<float?>(System.Linq.Queryable.Sum);

			public static readonly MethodInfo Sum_Int32_2 = GetMethodInfo_WithParameter<Expression<Func<object, int>>, int>(System.Linq.Queryable.Sum);
			public static readonly MethodInfo Sum_NullableInt32_2 = GetMethodInfo_WithParameter<Expression<Func<object, int?>>, int?>(System.Linq.Queryable.Sum);
			public static readonly MethodInfo Sum_Decimal_2 = GetMethodInfo_WithParameter<Expression<Func<object, decimal>>, decimal>(System.Linq.Queryable.Sum);
			public static readonly MethodInfo Sum_NullableDecimal_2 = GetMethodInfo_WithParameter<Expression<Func<object, decimal?>>, decimal?>(System.Linq.Queryable.Sum);
			public static readonly MethodInfo Sum_Double_2 = GetMethodInfo_WithParameter<Expression<Func<object, double>>, double>(System.Linq.Queryable.Sum);
			public static readonly MethodInfo Sum_NullableDouble_2 = GetMethodInfo_WithParameter<Expression<Func<object, double?>>, double?>(System.Linq.Queryable.Sum);
			public static readonly MethodInfo Sum_Long_2 = GetMethodInfo_WithParameter<Expression<Func<object, long>>, long>(System.Linq.Queryable.Sum);
			public static readonly MethodInfo Sum_NullableLong_2 = GetMethodInfo_WithParameter<Expression<Func<object, long?>>, long?>(System.Linq.Queryable.Sum);
			public static readonly MethodInfo Sum_Float_2 = GetMethodInfo_WithParameter<Expression<Func<object, float>>, float>(System.Linq.Queryable.Sum);
			public static readonly MethodInfo Sum_NullableFloat_2 = GetMethodInfo_WithParameter<Expression<Func<object, float?>>, float?>(System.Linq.Queryable.Sum);
		}

		public static class Enumerable
		{
			public static readonly MethodInfo First_1 = GetMethodInfo(System.Linq.Enumerable.First);
			public static readonly MethodInfo FirstOrDefault_1 = GetMethodInfo(System.Linq.Enumerable.FirstOrDefault);

			public static readonly MethodInfo Single_1 = GetMethodInfo(System.Linq.Enumerable.Single);
			public static readonly MethodInfo SingleOrDefault_1 = GetMethodInfo(System.Linq.Enumerable.SingleOrDefault);

			public static readonly MethodInfo Any_1 = GetMethodInfo(System.Linq.Enumerable.Any);
		}

		public static class AsyncEnumerable
		{
			public static readonly MethodInfo First_1 = GetMethodInfo(System.Linq.AsyncEnumerable.FirstAsync);
			public static readonly MethodInfo FirstOrDefault_1 = GetMethodInfo(System.Linq.AsyncEnumerable.FirstOrDefaultAsync);

			public static readonly MethodInfo Single_1 = GetMethodInfo(System.Linq.AsyncEnumerable.SingleAsync);
			public static readonly MethodInfo SingleOrDefault_1 = GetMethodInfo(System.Linq.AsyncEnumerable.SingleOrDefaultAsync);

			public static readonly MethodInfo Any_1 = GetMethodInfo(System.Linq.AsyncEnumerable.AnyAsync);
		}
	}
}
