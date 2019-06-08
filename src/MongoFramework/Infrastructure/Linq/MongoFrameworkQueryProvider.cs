using MongoDB.Driver.Linq;
using MongoFramework.Infrastructure.Diagnostics;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MongoFramework.Infrastructure.Linq
{
	public class MongoFrameworkQueryProvider<TEntity, TOutput> : IMongoFrameworkQueryProvider<TEntity, TOutput> where TEntity : class
	{
		private IMongoDbConnection Connection { get; }
		public IMongoQueryable UnderlyingQueryable { get; }

		public EntityProcessorCollection<TEntity> EntityProcessors { get; } = new EntityProcessorCollection<TEntity>();

		public MongoFrameworkQueryProvider(IMongoDbConnection connection, IMongoQueryable<TOutput> underlyingQueryable)
		{
			Connection = connection;
			UnderlyingQueryable = underlyingQueryable;
		}

		public IQueryable CreateQuery(Expression expression)
		{
			throw new NotImplementedException();
		}

		public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
		{
			var newUnderlyingQueryable = (IMongoQueryable<TElement>)UnderlyingQueryable.Provider.CreateQuery<TElement>(expression);
			var queryable = new MongoFrameworkQueryable<TEntity, TElement>(Connection, newUnderlyingQueryable, expression);
			queryable.EntityProcessors.AddRange(EntityProcessors);
			return queryable;
		}

		public virtual object Execute(Expression expression)
		{
			using (var diagnostics = DiagnosticRunner.Start(Connection, this))
			{
				object result;
				try
				{
					result = UnderlyingQueryable.Provider.Execute(expression);
				}
				catch (Exception exception)
				{
					diagnostics.Error(exception);
					throw;
				}

				if (result is TEntity)
				{
					EntityProcessors.ProcessEntity((TEntity)result, Connection);
				}

				return result;
			}
		}

		public virtual TResult Execute<TResult>(Expression expression)
		{
			var result = Execute(expression);
			return (TResult)result;
		}

		public virtual IEnumerable<TOutput> ExecuteEnumerable(Expression expression)
		{
			using (var diagnostics = DiagnosticRunner.Start(Connection, this))
			{
				IEnumerable<TOutput> result;
				try
				{
					result = (IEnumerable<TOutput>)UnderlyingQueryable.Provider.Execute(expression);
				}
				catch (Exception exception)
				{
					diagnostics.Error(exception);
					throw;
				}

				using (var enumerator = result.GetEnumerator())
				{
					var hasFirstResult = false;
					while (enumerator.MoveNext())
					{
						if (!hasFirstResult)
						{
							hasFirstResult = true;
							diagnostics.FirstReadResult<TOutput>();
						}

						var item = enumerator.Current;
						if (item is TEntity)
						{
							EntityProcessors.ProcessEntity((TEntity)(object)item, Connection);
						}
						yield return item;
					}
				}
			}
		}

		public string ToQuery()
		{
			var executionModel = UnderlyingQueryable.GetExecutionModel();
			var definition = EntityMapping.GetOrCreateDefinition(typeof(TEntity));
			return $"db.{definition.CollectionName}.{executionModel}";
		}
	}
}
