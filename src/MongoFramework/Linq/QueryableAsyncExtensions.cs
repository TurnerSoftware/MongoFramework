using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MongoFramework.Infrastructure.Linq;

namespace MongoFramework.Linq
{
	public static class QueryableAsyncExtensions
	{
		public static IAsyncEnumerable<TOutput> AsAsyncEnumerable<TOutput>(this IQueryable<TOutput> source, CancellationToken cancellationToken = default)
		{
			if (source.Provider is IMongoFrameworkQueryProvider asyncProvider)
			{
				var resultEnumerable = asyncProvider.ExecuteAsync(source.Expression, cancellationToken) as IAsyncEnumerable<TOutput>;
				return resultEnumerable;
			}

			throw new ArgumentException($"Query provider must implement interface {nameof(IMongoFrameworkQueryProvider)}", nameof(source));
		}

		public static async Task<TOutput[]> ToArrayAsync<TOutput>(this IQueryable<TOutput> source, CancellationToken cancellationToken = default)
		{
			return await source.AsAsyncEnumerable(cancellationToken).ToArrayAsync(cancellationToken);
		}

		public static async Task<List<TOutput>> ToListAsync<TOutput>(this IQueryable<TOutput> source, CancellationToken cancellationToken = default)
		{
			return await source.AsAsyncEnumerable(cancellationToken).ToListAsync(cancellationToken);
		}

		private static async Task<TResult> ExecuteExpressionAsync<TResult, TSource>(IQueryable<TSource> source, Expression expression, CancellationToken cancellationToken)
		{
			if (source.Provider is IMongoFrameworkQueryProvider provider)
			{
				var finalisedQueryable = provider.CreateQuery<TResult>(expression);
				var asyncProvider = finalisedQueryable.Provider as IMongoFrameworkQueryProvider;
				var resultTask = (ValueTask<TResult>)asyncProvider.ExecuteAsync(finalisedQueryable.Expression, cancellationToken);
				return await resultTask;
			}

			throw new ArgumentException($"Query provider must implement interface {nameof(IMongoFrameworkQueryProvider)}", nameof(source));
		}

		private static async Task<TResult> ExecuteMethodAsync<TResult, TSource>(IQueryable<TSource> source, MethodInfo method, CancellationToken cancellationToken)
		{
			return await ExecuteExpressionAsync<TResult, TSource>(source, Expression.Call(
				   null,
				   method,
				   source.Expression
			), cancellationToken);
		}
		private static async Task<TResult> ExecuteMethodAsync<TResult, TSource>(IQueryable<TSource> source, MethodInfo method, Expression secondArgument, CancellationToken cancellationToken)
		{
			return await ExecuteExpressionAsync<TResult, TSource>(source, Expression.Call(
				   null,
				   method,
				   source.Expression,
				   secondArgument
			), cancellationToken);
		}

		public static async Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<TSource, TSource>(source, MethodInfoCache.Queryable.First_1.MakeGenericMethod(typeof(TSource)), cancellationToken);
		}
		public static async Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<TSource, TSource>(source, MethodInfoCache.Queryable.First_2.MakeGenericMethod(typeof(TSource)), predicate, cancellationToken);
		}
		public static async Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<TSource, TSource>(source, MethodInfoCache.Queryable.FirstOrDefault_1.MakeGenericMethod(typeof(TSource)), cancellationToken);
		}
		public static async Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<TSource, TSource>(source, MethodInfoCache.Queryable.FirstOrDefault_2.MakeGenericMethod(typeof(TSource)), predicate, cancellationToken);
		}

		public static async Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<TSource, TSource>(source, MethodInfoCache.Queryable.Single_1.MakeGenericMethod(typeof(TSource)), cancellationToken);
		}
		public static async Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<TSource, TSource>(source, MethodInfoCache.Queryable.Single_2.MakeGenericMethod(typeof(TSource)), predicate, cancellationToken);
		}
		public static async Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<TSource, TSource>(source, MethodInfoCache.Queryable.SingleOrDefault_1.MakeGenericMethod(typeof(TSource)), cancellationToken);
		}
		public static async Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<TSource, TSource>(source, MethodInfoCache.Queryable.SingleOrDefault_2.MakeGenericMethod(typeof(TSource)), predicate, cancellationToken);
		}

		public static async Task<int> CountAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<int, TSource>(source, MethodInfoCache.Queryable.Count_1.MakeGenericMethod(typeof(TSource)), cancellationToken);
		}
		public static async Task<int> CountAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<int, TSource>(source, MethodInfoCache.Queryable.Count_2.MakeGenericMethod(typeof(TSource)), predicate, cancellationToken);
		}

		public static async Task<TSource> MaxAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<TSource, TSource>(source, MethodInfoCache.Queryable.Max_1.MakeGenericMethod(typeof(TSource)), cancellationToken);
		}
		public static async Task<TResult> MaxAsync<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<TResult, TSource>(source, MethodInfoCache.Queryable.Max_2.MakeGenericMethod(typeof(TSource), typeof(TResult)), selector, cancellationToken);
		}

		public static async Task<TSource> MinAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<TSource, TSource>(source, MethodInfoCache.Queryable.Min_1.MakeGenericMethod(typeof(TSource)), cancellationToken);
		}
		public static async Task<TResult> MinAsync<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<TResult, TSource>(source, MethodInfoCache.Queryable.Min_2.MakeGenericMethod(typeof(TSource), typeof(TResult)), selector, cancellationToken);
		}

		public static async Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<bool, TSource>(source, MethodInfoCache.Queryable.Any_1.MakeGenericMethod(typeof(TSource)), cancellationToken);
		}
		public static async Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<bool, TSource>(source, MethodInfoCache.Queryable.Any_2.MakeGenericMethod(typeof(TSource)), predicate, cancellationToken);
		}

		public static async Task<int> SumAsync(this IQueryable<int> source, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<int, int>(source, MethodInfoCache.Queryable.Sum_Int32_1, cancellationToken);
		}
		public static async Task<int?> SumAsync(this IQueryable<int?> source, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<int?, int?>(source, MethodInfoCache.Queryable.Sum_NullableInt32_1, cancellationToken);
		}
		public static async Task<decimal> SumAsync(this IQueryable<decimal> source, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<decimal, decimal>(source, MethodInfoCache.Queryable.Sum_Decimal_1, cancellationToken);
		}
		public static async Task<decimal?> SumAsync(this IQueryable<decimal?> source, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<decimal?, decimal?>(source, MethodInfoCache.Queryable.Sum_NullableDecimal_1, cancellationToken);
		}
		public static async Task<double> SumAsync(this IQueryable<double> source, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<double, double>(source, MethodInfoCache.Queryable.Sum_Double_1, cancellationToken);
		}
		public static async Task<double?> SumAsync(this IQueryable<double?> source, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<double?, double?>(source, MethodInfoCache.Queryable.Sum_NullableDouble_1, cancellationToken);
		}
		public static async Task<float> SumAsync(this IQueryable<float> source, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<float, float>(source, MethodInfoCache.Queryable.Sum_Float_1, cancellationToken);
		}
		public static async Task<float?> SumAsync(this IQueryable<float?> source, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<float?, float?>(source, MethodInfoCache.Queryable.Sum_NullableFloat_1, cancellationToken);
		}
		public static async Task<long> SumAsync(this IQueryable<long> source, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<long, long>(source, MethodInfoCache.Queryable.Sum_Long_1, cancellationToken);
		}
		public static async Task<long?> SumAsync(this IQueryable<long?> source, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<long?, long?>(source, MethodInfoCache.Queryable.Sum_NullableLong_1, cancellationToken);
		}

		public static async Task<int> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int>> selector, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<int, TSource>(source, MethodInfoCache.Queryable.Sum_Int32_2.MakeGenericMethod(typeof(TSource)), selector, cancellationToken);
		}
		public static async Task<int?> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<int?, TSource>(source, MethodInfoCache.Queryable.Sum_NullableInt32_2.MakeGenericMethod(typeof(TSource)), selector, cancellationToken);
		}
		public static async Task<decimal> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<decimal, TSource>(source, MethodInfoCache.Queryable.Sum_Decimal_2.MakeGenericMethod(typeof(TSource)), selector, cancellationToken);
		}
		public static async Task<decimal?> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<decimal?, TSource>(source, MethodInfoCache.Queryable.Sum_NullableDecimal_2.MakeGenericMethod(typeof(TSource)), selector, cancellationToken);
		}
		public static async Task<double> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double>> selector, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<double, TSource>(source, MethodInfoCache.Queryable.Sum_Double_2.MakeGenericMethod(typeof(TSource)), selector, cancellationToken);
		}
		public static async Task<double?> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<double?, TSource>(source, MethodInfoCache.Queryable.Sum_NullableDouble_2.MakeGenericMethod(typeof(TSource)), selector, cancellationToken);
		}
		public static async Task<float> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float>> selector, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<float, TSource>(source, MethodInfoCache.Queryable.Sum_Float_2.MakeGenericMethod(typeof(TSource)), selector, cancellationToken);
		}
		public static async Task<float?> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<float?, TSource>(source, MethodInfoCache.Queryable.Sum_NullableFloat_2.MakeGenericMethod(typeof(TSource)), selector, cancellationToken);
		}
		public static async Task<long> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long>> selector, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<long, TSource>(source, MethodInfoCache.Queryable.Sum_Long_2.MakeGenericMethod(typeof(TSource)), selector, cancellationToken);
		}
		public static async Task<long?> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<long?, TSource>(source, MethodInfoCache.Queryable.Sum_NullableLong_2.MakeGenericMethod(typeof(TSource)), selector, cancellationToken);
		}
	}
}
