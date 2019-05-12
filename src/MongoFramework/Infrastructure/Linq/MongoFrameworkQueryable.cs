using MongoDB.Driver.Linq;
using MongoFramework.Infrastructure.Diagnostics;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MongoFramework.Infrastructure.Linq
{
	public class MongoFrameworkQueryable<TEntity, TOutput> : IMongoFrameworkQueryable<TEntity, TOutput> where TEntity : class
	{
		private IMongoFrameworkQueryProvider<TEntity, TOutput> InternalProvider { get; set; }

		public IMongoDbConnection Connection { get; }
		public Type ElementType => typeof(TOutput);
		public Expression Expression { get; }
		public IQueryProvider Provider => InternalProvider;

		public EntityProcessorCollection<TEntity> EntityProcessors => InternalProvider.EntityProcessors;

		public MongoFrameworkQueryable(IMongoDbConnection connection, IMongoQueryable<TOutput> underlyingQueryable)
		{
			Connection = connection;
			InternalProvider = new MongoFrameworkQueryProvider<TEntity, TOutput>(connection, underlyingQueryable);
			Expression = Expression.Constant(underlyingQueryable, typeof(IMongoQueryable<TOutput>));
		}

		public MongoFrameworkQueryable(IMongoDbConnection connection, IMongoQueryable<TOutput> underlyingQueryable, Expression expression)
		{
			Connection = connection;
			InternalProvider = new MongoFrameworkQueryProvider<TEntity, TOutput>(connection, underlyingQueryable);
			Expression = expression;
		}

		public IEnumerator<TOutput> GetEnumerator()
		{
			var commandId = Guid.NewGuid();

			var errored = false;
			try
			{
				Connection.DiagnosticListener.OnNext(new ReadDiagnosticCommand
				{
					CommandId = commandId,
					Source = $"{nameof(MongoFrameworkQueryable<TEntity, TOutput>)}.{nameof(GetEnumerator)}",
					CommandState = CommandState.Start,
					EntityType = typeof(TEntity),
					Queryable = this
				});

				IEnumerable<TOutput> result;
				try
				{
					result = (IEnumerable<TOutput>)InternalProvider.Execute(Expression);
					Connection.DiagnosticListener.OnNext(new ReadDiagnosticCommand
					{
						CommandId = commandId,
						Source = $"{nameof(MongoFrameworkQueryable<TEntity, TOutput>)}.{nameof(GetEnumerator)}",
						CommandState = CommandState.FirstResult, //Note: May need to move this around to actually be after the first "MoveNext" or something
						EntityType = typeof(TEntity),
						Queryable = this
					});
				}
				catch (Exception ex)
				{
					errored = true;
					Connection.DiagnosticListener.OnNext(new ReadDiagnosticCommand
					{
						CommandId = commandId,
						Source = $"{nameof(MongoFrameworkQueryable<TEntity, TOutput>)}.{nameof(GetEnumerator)}",
						CommandState = CommandState.Error,
						EntityType = typeof(TEntity),
						Queryable = this
					});
					Connection.DiagnosticListener.OnError(ex);

					throw;
				}

				using (var enumerator = result.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						yield return enumerator.Current;
					}
				}
			}
			finally
			{
				if (!errored)
				{
					Connection.DiagnosticListener.OnNext(new ReadDiagnosticCommand
					{
						CommandId = commandId,
						Source = $"{nameof(MongoFrameworkQueryable<TEntity, TOutput>)}.{nameof(GetEnumerator)}",
						CommandState = CommandState.End,
						EntityType = typeof(TEntity),
						Queryable = this
					});
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public string ToQuery()
		{
			var executionModel = InternalProvider.UnderlyingQueryable.GetExecutionModel();
			var definition = EntityMapping.GetOrCreateDefinition(typeof(TEntity));
			return $"db.{definition.CollectionName}.{executionModel}";
		}
	}
}
