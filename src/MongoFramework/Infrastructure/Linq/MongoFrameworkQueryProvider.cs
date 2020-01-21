﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoFramework.Infrastructure.Diagnostics;
using MongoFramework.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MongoFramework.Infrastructure.Linq
{
	public class MongoFrameworkQueryProvider<TEntity> : IMongoFrameworkQueryProvider<TEntity> where TEntity : class
	{
		public IMongoDbConnection Connection { get; }
		private IEntityDefinition EntityDefinition { get; }
		private BsonDocument PreStage { get; }
		public EntityProcessorCollection<TEntity> EntityProcessors { get; } = new EntityProcessorCollection<TEntity>();
		public MongoFrameworkQueryProvider(IMongoDbConnection connection) : this(connection, null) { }
		public MongoFrameworkQueryProvider(IMongoDbConnection connection, BsonDocument preStage)
		{
			Connection = connection;
			EntityDefinition = EntityMapping.GetOrCreateDefinition(typeof(TEntity));
			PreStage = preStage;
		}
		public MongoFrameworkQueryProvider(IMongoFrameworkQueryProvider<TEntity> provider, BsonDocument preStage) : this(provider.Connection, preStage)
		 => EntityProcessors.AddRange(provider.EntityProcessors);

		public Expression GetBaseExpression()
		{
			var collection = GetCollection();
			return Expression.Constant(collection.AsQueryable(), typeof(IMongoQueryable<TEntity>));
		}

		public IQueryable CreateQuery(Expression expression) => throw new NotImplementedException();
		public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new MongoFrameworkQueryable<TElement>(this, expression);
		public object Execute(Expression expression)
		{
			var model = GetExecutionModel(expression);
			var outputType = model.Serializer.ValueType;

			Expression executor = Expression.Call(
				Expression.Constant(this),
				nameof(ExecuteModel),
				new[] { outputType },
				Expression.Constant(model, typeof(AggregateExecutionModel)));

			if (model.ResultTransformer != null)
			{
				executor = Expression.Invoke(model.ResultTransformer, executor);
			}

			var lambda = Expression.Lambda(executor);

			try
			{
				return lambda.Compile().DynamicInvoke(null);
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}
		public TResult Execute<TResult>(Expression expression) => (TResult)Execute(expression);
		private IMongoCollection<TEntity> GetCollection() => Connection.GetDatabase().GetCollection<TEntity>(EntityDefinition.CollectionName);
		private AggregateExecutionModel GetExecutionModel(Expression expression)
		{
			var underlyingProvider = GetCollection().AsQueryable().Provider;
			var providerType = underlyingProvider.GetType();

			var translatedQuery = providerType.GetMethod("Translate", BindingFlags.NonPublic | BindingFlags.Instance)
				.Invoke(underlyingProvider, new[] { expression });
			var translatedQueryType = translatedQuery.GetType();

			//Get the model stages and processing type
			var underlyingExecutionModel = translatedQueryType.GetProperty("Model").GetValue(translatedQuery) as QueryableExecutionModel;
			var modelType = underlyingExecutionModel.GetType(); //Assumed type: AggregateQueryableExecutionModel<>
			var expressionStages = modelType.GetProperty(nameof(AggregateQueryableExecutionModel<object>.Stages))
					.GetValue(underlyingExecutionModel) as IEnumerable<BsonDocument>;
			var serializer = modelType.GetProperty(nameof(AggregateQueryableExecutionModel<object>.OutputSerializer))
					.GetValue(underlyingExecutionModel) as IBsonSerializer;

			if (PreStage != null)
			{
				expressionStages = new[] { PreStage }.Concat(expressionStages);
			}

			var result = new AggregateExecutionModel
			{
				Stages = expressionStages,
				Serializer = serializer
			};

			//Get the result transforming lambda (allows things like FirstOrDefault, Count, Average etc to work properly)
			var resultTransformer = translatedQueryType.GetProperty("ResultTransformer").GetValue(translatedQuery);
			if (resultTransformer != null)
			{
				var resultTransformerType = resultTransformer.GetType();
				var lambda = resultTransformerType.GetMethod("CreateAggregator").Invoke(resultTransformer, new[] { serializer.ValueType });
				result.ResultTransformer = lambda as LambdaExpression;
			}

			return result;
		}
		private IEnumerable<TResult> ExecuteModel<TResult>(AggregateExecutionModel model)
		{
			var serializer = model.Serializer as IBsonSerializer<TResult>;
			var pipeline = PipelineDefinition<TEntity, TResult>.Create(model.Stages, serializer);
			using (var diagnostics = DiagnosticRunner.Start<TEntity>(Connection, model))
			{
				IEnumerable<TResult> underlyingResult;

				try
				{
					underlyingResult = GetCollection().Aggregate(pipeline).ToEnumerable();
				}
				catch (Exception exception)
				{
					diagnostics.Error(exception);
					throw;
				}

				using (var enumerator = underlyingResult.GetEnumerator())
				{
					var hasFirstResult = false;
					while (enumerator.MoveNext())
					{
						if (!hasFirstResult)
						{
							hasFirstResult = true;
							diagnostics.FirstReadResult<TResult>();
						}

						var item = enumerator.Current;
						if (item is TEntity entityItem)
						{
							EntityProcessors.ProcessEntity(entityItem, Connection);
						}
						yield return item;
					}
				}
			}
		}
		public string ToQuery(Expression expression)
		{
			var model = GetExecutionModel(expression);
			return QueryHelper.GetQuery<TEntity>(model);
		}
	}
}
