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
			if (source is IMongoFrameworkQueryable)
			{
				var asyncProvider = source.Provider as IMongoFrameworkQueryProvider;
				var resultEnumerable = asyncProvider.ExecuteAsync(source.Expression, cancellationToken) as IAsyncEnumerable<TOutput>;
				return resultEnumerable;
			}

			throw new ArgumentException($"Queryable must implement interface {nameof(IMongoFrameworkQueryable)}", nameof(source));
		}

		private static async Task<TResult> ExecuteExpressionAsync<TResult, TSource>(IQueryable<TSource> source, Expression expression, CancellationToken cancellationToken)
		{
			if (source is IMongoFrameworkQueryable)
			{
				var finalisedQueryable = source.Provider.CreateQuery<TResult>(expression) as IMongoFrameworkQueryable;
				var asyncProvider = finalisedQueryable.Provider as IMongoFrameworkQueryProvider;
				var resultTask = (ValueTask<TResult>)asyncProvider.ExecuteAsync(finalisedQueryable.Expression, cancellationToken);
				return await resultTask;
			}

			throw new ArgumentException($"Queryable must implement interface {nameof(IMongoFrameworkQueryable)}", nameof(source));
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

		public static async Task<int> CountAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<int, TSource>(source, MethodInfoCache.Queryable.Count_1.MakeGenericMethod(typeof(TSource)), cancellationToken);
		}
		public static async Task<int> CountAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
		{
			return await ExecuteMethodAsync<int, TSource>(source, MethodInfoCache.Queryable.Count_2.MakeGenericMethod(typeof(TSource)),	predicate, cancellationToken);
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
	}
}
