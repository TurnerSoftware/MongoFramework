using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoFramework.Infrastructure.Linq;

namespace MongoFramework.Linq
{
	public static class LinqAsyncExtensions
	{
		public static IAsyncEnumerable<TOutput> AsAsyncEnumerable<TOutput>(this IQueryable<TOutput> source, CancellationToken cancellationToken = default)
		{
			if (source is IMongoFrameworkQueryable)
			{
				var asyncProvider = source.Provider as IMongoFrameworkQueryProvider;
				var resultEnumerable = asyncProvider.ExecuteAsync(source.Expression, cancellationToken) as IAsyncEnumerable<TOutput>;
				return resultEnumerable;
			}

			throw new ArgumentException($"Queryable must implement interface {nameof(IMongoFrameworkQueryable)}", nameof(source));
		}

		private static async Task<TResult> ExecuteAsync<TResult, TSource>(IQueryable<TSource> source, Expression expression, CancellationToken cancellationToken = default)
		{
			if (source is IMongoFrameworkQueryable)
			{
				var finalisedQueryable = source.Provider.CreateQuery<TResult>(expression) as IMongoFrameworkQueryable;
				var asyncProvider = finalisedQueryable.Provider as IMongoFrameworkQueryProvider;
				var resultTask = asyncProvider.ExecuteAsync(finalisedQueryable.Expression, cancellationToken) as Task<TResult>;
				return await resultTask;
			}

			throw new ArgumentException($"Queryable must implement interface {nameof(IMongoFrameworkQueryable)}", nameof(source));
		}

		public static async Task<int> CountAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
		{
			return await ExecuteAsync<int, TSource>(source, Expression.Call(
				null,
				QueryableMethods.Count_TSource_1(typeof(TSource)),
				source.Expression
			), cancellationToken);
		}
		public static async Task<int> CountAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
		{
			return await ExecuteAsync<int, TSource>(source, Expression.Call(
				null,
				QueryableMethods.Count_TSource_2(typeof(TSource)),
				source.Expression,
				predicate
			), cancellationToken);
		}

		public static async Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
		{
			return await ExecuteAsync<TSource, TSource>(source, Expression.Call(
				null,
				QueryableMethods.First_TSource_1(typeof(TSource)),
				source.Expression
			), cancellationToken);
		}
		public static async Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
		{
			return await ExecuteAsync<TSource, TSource>(source, Expression.Call(
				null,
				QueryableMethods.First_TSource_2(typeof(TSource)),
				source.Expression,
				predicate
			), cancellationToken);
		}

		public static async Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
		{
			return await ExecuteAsync<TSource, TSource>(source, Expression.Call(
				null,
				QueryableMethods.FirstOrDefault_TSource_1(typeof(TSource)),
				source.Expression
			), cancellationToken);
		}
		public static async Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
		{
			return await ExecuteAsync<TSource, TSource>(source, Expression.Call(
				null,
				QueryableMethods.FirstOrDefault_TSource_2(typeof(TSource)),
				source.Expression,
				predicate
			), cancellationToken);
		}
	}
}
