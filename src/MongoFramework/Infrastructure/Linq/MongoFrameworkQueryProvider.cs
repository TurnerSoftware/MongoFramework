using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoFramework.Infrastructure.Diagnostics;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Infrastructure.Linq
{
	public class MongoFrameworkQueryProvider<TEntity> : IMongoFrameworkQueryProvider<TEntity> where TEntity : class
	{
		public IMongoDbConnection Connection { get; }
		private EntityDefinition EntityDefinition { get; }

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
		{
			EntityProcessors.AddRange(provider.EntityProcessors);
		}

		public Expression GetBaseExpression()
		{
			var collection = GetCollection();
			var queryable = collection.AsQueryable();
			// Use the actual queryable type so the driver can recognize it
			return Expression.Constant(queryable, queryable.GetType());
		}

		public IQueryable CreateQuery(Expression expression)
		{
			throw new NotImplementedException();
		}

		public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
		{
			return new MongoFrameworkQueryable<TElement>(this, expression);
		}

		/// <summary>
		/// Returns true if the connection has a diagnostic listener that requires aggregation-based execution
		/// to properly capture query profiling.
		/// </summary>
		private bool HasDiagnosticsEnabled()
		{
			return Connection.DiagnosticListener != null && !(Connection.DiagnosticListener is NoOpDiagnosticListener);
		}

		public object Execute(Expression expression)
		{
			// If there's a PreStage (e.g., for $geoNear, multi-tenant filtering), we must use
			// aggregation-based execution to properly apply it.
			// Also use aggregation path when diagnostics are enabled to properly capture query profiling.
			if (PreStage != null || HasDiagnosticsEnabled())
			{
				return ExecuteViaAggregation(expression);
			}

			// In MongoDB.Driver 3.x, extract the driver's queryable and execute through it
			var queryableAndProvider = ExtractQueryableFromExpression(expression);

			if (queryableAndProvider.queryable != null)
			{
				var driverQueryable = queryableAndProvider.queryable;
				var driverProvider = queryableAndProvider.provider;

				// If the expression is just a ConstantExpression (the base queryable),
				// enumerate the queryable directly
				if (expression is ConstantExpression)
				{
					return EnumerateAndProcess(driverQueryable);
				}

				// Rebase the expression to use the driver's queryable (handles MongoFrameworkQueryable wrappers)
				var rebasedExpression = RebaseExpression(expression, driverQueryable);

				// Execute through the driver's provider directly
				var resultType = expression.Type;
				var executeMethod = driverProvider.GetType()
					.GetMethods()
					.FirstOrDefault(m => m.Name == "Execute" && m.IsGenericMethodDefinition);

				if (executeMethod != null)
				{
					try
					{
						var genericExecute = executeMethod.MakeGenericMethod(resultType);
						var result = genericExecute.Invoke(driverProvider, new object[] { rebasedExpression });

						// Process entities if result is a single entity
						if (result is TEntity entity)
						{
							EntityProcessors.ProcessEntity(entity, Connection);
						}
						else if (result is IEnumerable<TEntity> entities)
						{
							// Materialize and process
							var list = new List<TEntity>();
							foreach (var e in entities)
							{
								EntityProcessors.ProcessEntity(e, Connection);
								list.Add(e);
							}
							return list;
						}

						return result;
					}
					catch (TargetInvocationException ex)
					{
						var innerEx = ex.InnerException;
						// If the inner exception is a business logic exception, throw it
						// If it's an expression translation exception, fall back to aggregation
						if (innerEx is InvalidOperationException ||
							innerEx is ArgumentException ||
							innerEx is ArgumentNullException)
						{
							throw innerEx;
						}
						// For other exceptions (expression not supported, etc.), fall back
					}
				}
			}

			// Fallback: Use aggregation-based execution
			return ExecuteViaAggregation(expression);
		}

		/// <summary>
		/// Executes the expression using aggregation pipeline. Required when PreStage is set
		/// or when direct driver delegation fails.
		/// </summary>
		private object ExecuteViaAggregation(Expression expression)
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

		/// <summary>
		/// Replaces the source ConstantExpression in an expression tree with the driver queryable's own expression.
		/// </summary>
		private static Expression RebaseExpression(Expression expression, IQueryable driverQueryable)
		{
			return new QueryableExpressionReplacer(driverQueryable.Expression).Visit(expression);
		}

		private class QueryableExpressionReplacer : System.Linq.Expressions.ExpressionVisitor
		{
			private readonly Expression _replacement;

			public QueryableExpressionReplacer(Expression replacement)
			{
				_replacement = replacement;
			}

			protected override Expression VisitConstant(ConstantExpression node)
			{
				// Check if this is a queryable constant
				if (node.Value != null)
				{
					var valueType = node.Value.GetType();
					var providerProperty = valueType.GetProperty("Provider");
					if (providerProperty != null)
					{
						// This is a queryable, replace it with the driver's expression
						return _replacement;
					}
				}
				return base.VisitConstant(node);
			}
		}

		public TResult Execute<TResult>(Expression expression)
		{
			return (TResult)Execute(expression);
		}

		public object ExecuteAsync(Expression expression, CancellationToken cancellationToken = default)
		{
			// If there's a PreStage (e.g., for $geoNear, multi-tenant filtering), we must use
			// aggregation-based execution to properly apply it.
			// Also use aggregation path when diagnostics are enabled to properly capture query profiling.
			if (PreStage != null || HasDiagnosticsEnabled())
			{
				return ExecuteAsyncViaAggregation(expression, cancellationToken);
			}

			// In MongoDB.Driver 3.x, delegate to the driver's provider for async execution
			var queryableAndProvider = ExtractQueryableFromExpression(expression);

			if (queryableAndProvider.queryable != null)
			{
				var driverQueryable = queryableAndProvider.queryable;
				var driverProvider = queryableAndProvider.provider;

				// If the expression is just a ConstantExpression (the base queryable),
				// return an async enumerable that enumerates the driver's queryable
				if (expression is ConstantExpression)
				{
					return EnumerateAndProcessAsync(driverQueryable, cancellationToken);
				}

				// Rebase the expression to use the driver's queryable
				var rebasedExpression = RebaseExpression(expression, driverQueryable);

				// Execute through the driver's provider directly
				var resultType = expression.Type;
				var executeMethod = driverProvider.GetType()
					.GetMethods()
					.FirstOrDefault(m => m.Name == "Execute" && m.IsGenericMethodDefinition);

				if (executeMethod != null)
				{
					try
					{
						var genericExecute = executeMethod.MakeGenericMethod(resultType);
						var syncResult = genericExecute.Invoke(driverProvider, new object[] { rebasedExpression });

						// Process entities
						if (syncResult is TEntity entity)
						{
							EntityProcessors.ProcessEntity(entity, Connection);
						}

						// Return as ValueTask<TResult>
						return CreateValueTask(syncResult, resultType);
					}
					catch (TargetInvocationException ex)
					{
						var innerEx = ex.InnerException;
						// If the inner exception is a business logic exception, throw it
						// If it's an expression translation exception, fall back to aggregation
						if (innerEx is InvalidOperationException ||
							innerEx is ArgumentException ||
							innerEx is ArgumentNullException)
						{
							throw innerEx;
						}
						// For other exceptions (expression not supported, etc.), fall back
					}
				}
			}

			// Fallback to aggregation-based execution
			return ExecuteAsyncViaAggregation(expression, cancellationToken);
		}

		/// <summary>
		/// Executes the expression asynchronously using aggregation pipeline. Required when PreStage is set
		/// or when direct driver delegation fails.
		/// </summary>
		private object ExecuteAsyncViaAggregation(Expression expression, CancellationToken cancellationToken)
		{
			var model = GetExecutionModel(expression, true);
			var outputType = model.Serializer.ValueType;

			Expression executor = Expression.Call(
				Expression.Constant(this),
				nameof(ExecuteModelAsync),
				new[] { outputType },
				Expression.Constant(model, typeof(AggregateExecutionModel)),
				Expression.Constant(cancellationToken));

			if (model.ResultTransformer != null)
			{
				executor = Expression.Invoke(
					model.ResultTransformer,
					Expression.Convert(executor, model.ResultTransformer.Parameters[0].Type),
					Expression.Constant(cancellationToken)
				);
			}

			var lambda = Expression.Lambda(executor);
			return lambda.Compile().DynamicInvoke(null);
		}

		private IMongoCollection<TEntity> GetCollection()
		{
			return Connection.GetDatabase().GetCollection<TEntity>(EntityDefinition.CollectionName);
		}

		private AggregateExecutionModel GetExecutionModel(Expression expression, bool isAsync = false)
		{
			// MongoDB.Driver 3.x: Use LINQ3 translation path
			// Extract the underlying queryable and provider from the expression's source
			var (driverQueryable, underlyingProvider) = ExtractQueryableFromExpression(expression);
			if (underlyingProvider == null)
			{
				driverQueryable = GetCollection().AsQueryable();
				underlyingProvider = driverQueryable.Provider;
			}

			// Rebase the expression to use the driver's queryable before translation
			// This is critical - the driver's translator won't recognize MongoFrameworkQueryable
			var rebasedExpression = RebaseExpression(expression, driverQueryable);

			// Use reflection to access the internal TranslateAndGetExecutionModel method
			// which handles the expression translation properly
			var providerType = underlyingProvider.GetType();

			// Try to find and use the TranslateExpressionToAggregateQueryPipeline method or similar
			// This is a more direct way to get the pipeline stages from the driver
			var driverAssembly = typeof(MongoClient).Assembly;

			// Get the Translate method from the provider itself (if available in 3.x)
			var translateMethod = providerType.GetMethod("Translate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			if (translateMethod != null)
			{
				// Driver 3.x might still have a Translate method on the provider
				var translatedQuery = translateMethod.Invoke(underlyingProvider, new[] { rebasedExpression });
				var translatedQueryType = translatedQuery.GetType();

				// Try to get the execution model or pipeline from the translated query
				var modelProperty = translatedQueryType.GetProperty("Model");
				if (modelProperty != null)
				{
					var executionModel = modelProperty.GetValue(translatedQuery);
					var modelType = executionModel.GetType();

					var stagesProperty = modelType.GetProperty("Stages");
					var serializerProperty = modelType.GetProperty("OutputSerializer");

					var expressionStages = stagesProperty?.GetValue(executionModel) as IEnumerable<BsonDocument>
						?? Array.Empty<BsonDocument>();
					var serializer = serializerProperty?.GetValue(executionModel) as IBsonSerializer;

					if (PreStage != null)
					{
						expressionStages = new[] { PreStage }.Concat(expressionStages);
					}

					var result = new AggregateExecutionModel
					{
						Stages = expressionStages,
						Serializer = serializer
					};

					// Get result transformer
					var resultTransformerProperty = translatedQueryType.GetProperty("ResultTransformer");
					var resultTransformer = resultTransformerProperty?.GetValue(translatedQuery);
					if (resultTransformer != null)
					{
						result.ResultTransformer = ResultTransformers.Transform(expression, serializer.ValueType, isAsync) as LambdaExpression;
					}

					return result;
				}
			}

			// Fallback: Use the static ExpressionToExecutableQueryTranslator.Translate method
			var translatorType = driverAssembly.GetType(
				"MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToExecutableQueryTranslators.ExpressionToExecutableQueryTranslator");

			if (translatorType == null)
			{
				throw new InvalidOperationException("Could not find ExpressionToExecutableQueryTranslator type. Ensure you are using MongoDB.Driver 3.x or later.");
			}

			var getTranslationOptionsMethod = providerType.GetMethod("GetTranslationOptions", BindingFlags.NonPublic | BindingFlags.Instance);
			var translationOptions = getTranslationOptionsMethod?.Invoke(underlyingProvider, null);

			var resultType = GetResultType(expression);

			var translateMethods = translatorType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
				.Where(m => m.Name == "Translate" && m.IsGenericMethodDefinition);
			var translateMethodDefinition = translateMethods.FirstOrDefault();

			if (translateMethodDefinition == null)
			{
				throw new InvalidOperationException("Could not find Translate method on ExpressionToExecutableQueryTranslator");
			}

			var genericTranslateMethod = translateMethodDefinition.MakeGenericMethod(typeof(TEntity), resultType);
			var executableQuery = genericTranslateMethod.Invoke(null, new[] { underlyingProvider, rebasedExpression, translationOptions });
			var executableQueryType = executableQuery.GetType();

			var pipelineProperty = executableQueryType.GetProperty("Pipeline", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			var pipeline = pipelineProperty.GetValue(executableQuery);
			var pipelineType = pipeline.GetType();

			var outputSerializerProperty = pipelineType.GetProperty("OutputSerializer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			var serializer2 = outputSerializerProperty.GetValue(pipeline) as IBsonSerializer;

			var astProperty = pipelineType.GetProperty("Ast", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			var ast = astProperty.GetValue(pipeline);
			var astType = ast.GetType();

			var renderMethod = astType.GetMethod("Render", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
			var renderedPipeline = renderMethod.Invoke(ast, null) as BsonValue;

			IEnumerable<BsonDocument> expressionStages2;
			if (renderedPipeline is BsonArray bsonArray)
			{
				expressionStages2 = bsonArray.Select(v => v.AsBsonDocument).ToArray();
			}
			else
			{
				expressionStages2 = Array.Empty<BsonDocument>();
			}

			if (PreStage != null)
			{
				expressionStages2 = new[] { PreStage }.Concat(expressionStages2);
			}

			var result2 = new AggregateExecutionModel
			{
				Stages = expressionStages2,
				Serializer = serializer2
			};

			// Determine if this is a scalar query (First, Count, Any, etc.)
			// In LINQ3, we detect this by checking if the expression is a MethodCallExpression with a scalar method
			if (expression is MethodCallExpression methodCall)
			{
				var methodName = methodCall.Method.Name;
				var scalarMethods = new[] { "First", "FirstOrDefault", "Single", "SingleOrDefault", "Last", "LastOrDefault",
					"Count", "LongCount", "Any", "All", "Sum", "Average", "Min", "Max", "ElementAt", "ElementAtOrDefault" };

				if (scalarMethods.Contains(methodName))
				{
					result2.ResultTransformer = ResultTransformers.Transform(expression, serializer2.ValueType, isAsync) as LambdaExpression;
				}
			}

			return result2;
		}

		/// <summary>
		/// Determines the result element type from the expression.
		/// For IQueryable&lt;T&gt; expressions, returns T.
		/// For scalar methods like Count(), returns the appropriate type.
		/// </summary>
		private static Type GetResultType(Expression expression)
		{
			var expressionType = expression.Type;

			// Check if it's IQueryable<T> or IEnumerable<T>
			if (expressionType.IsGenericType)
			{
				var genericDef = expressionType.GetGenericTypeDefinition();
				if (genericDef == typeof(IQueryable<>) || genericDef == typeof(IEnumerable<>) || genericDef == typeof(IOrderedQueryable<>))
				{
					return expressionType.GetGenericArguments()[0];
				}
			}

			// For scalar results (Count, Sum, etc.), the type is the expression's type itself
			// but we need to find the element type of the source
			if (expression is MethodCallExpression methodCall && methodCall.Arguments.Count > 0)
			{
				// Get the source type from the first argument
				var sourceType = methodCall.Arguments[0].Type;
				if (sourceType.IsGenericType)
				{
					var sourceGenericDef = sourceType.GetGenericTypeDefinition();
					if (sourceGenericDef == typeof(IQueryable<>) || sourceGenericDef == typeof(IEnumerable<>) || sourceGenericDef == typeof(IOrderedQueryable<>))
					{
						return sourceType.GetGenericArguments()[0];
					}
				}
			}

			// Fallback to the expression type
			return expressionType;
		}

		/// <summary>
		/// Extracts the underlying MongoDB driver's IQueryable and IQueryProvider from an expression tree.
		/// This method unwraps MongoFrameworkQueryable to find the actual driver queryable.
		/// </summary>
		private static (IQueryable queryable, IQueryProvider provider) ExtractQueryableFromExpression(Expression expression)
		{
			// Walk down the expression tree to find the root ConstantExpression
			var current = expression;
			while (current != null)
			{
				if (current is ConstantExpression constant)
				{
					var value = constant.Value;
					if (value != null)
					{
						// Check if this is a MongoFrameworkQueryable - if so, dig into its expression
						// to find the actual driver queryable
						if (value is IMongoFrameworkQueryable frameworkQueryable)
						{
							// Recursively extract from the framework queryable's expression
							// which should contain the driver's queryable
							return ExtractQueryableFromExpression(frameworkQueryable.Expression);
						}

						// Check if this is the driver's queryable (IMongoQueryable or similar)
						if (value is IQueryable queryable)
						{
							// Make sure it's not another MongoFrameworkQueryable
							if (!(queryable.Provider is IMongoFrameworkQueryProvider))
							{
								return (queryable, queryable.Provider);
							}
						}
					}
				}

				if (current is MethodCallExpression methodCall && methodCall.Arguments.Count > 0)
				{
					// The first argument of Queryable extension methods is the source
					current = methodCall.Arguments[0];
				}
				else
				{
					break;
				}
			}

			return (null, null);
		}

		/// <summary>
		/// Creates a ValueTask&lt;T&gt; with the given result value, handling null values correctly.
		/// </summary>
		private static object CreateValueTask(object result, Type resultType)
		{
			// Use a generic method to properly create the ValueTask<T>
			var method = typeof(MongoFrameworkQueryProvider<TEntity>)
				.GetMethod(nameof(CreateValueTaskGeneric), BindingFlags.NonPublic | BindingFlags.Static)
				.MakeGenericMethod(resultType);
			return method.Invoke(null, new[] { result });
		}

		private static ValueTask<T> CreateValueTaskGeneric<T>(object result)
		{
			return new ValueTask<T>((T)result);
		}

		/// <summary>
		/// Legacy method for compatibility - extracts just the provider.
		/// </summary>
		private static IQueryProvider ExtractProviderFromExpression(Expression expression)
		{
			return ExtractQueryableFromExpression(expression).provider;
		}

		/// <summary>
		/// Enumerates the driver's queryable and processes each entity.
		/// Used when the expression is just the base queryable (not a method call).
		/// </summary>
		private IEnumerable<TEntity> EnumerateAndProcess(IQueryable driverQueryable)
		{
			foreach (var item in driverQueryable)
			{
				if (item is TEntity entity)
				{
					EntityProcessors.ProcessEntity(entity, Connection);
					yield return entity;
				}
			}
		}

		/// <summary>
		/// Async version of EnumerateAndProcess.
		/// </summary>
		private async IAsyncEnumerable<TEntity> EnumerateAndProcessAsync(IQueryable driverQueryable, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			// MongoDB driver's IMongoQueryable implements IAsyncCursorSource, but for simplicity
			// we use sync enumeration here (wrapped in async). For true async, we'd need to use
			// the driver's ToCursorAsync method.
			foreach (var item in driverQueryable)
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (item is TEntity entity)
				{
					EntityProcessors.ProcessEntity(entity, Connection);
					yield return entity;
				}
			}
		}

		private IEnumerable<TResult> ExecuteModel<TResult>(AggregateExecutionModel model)
		{
			var serializer = model.Serializer as IBsonSerializer<TResult>;
			var pipeline = PipelineDefinition<TEntity, TResult>.Create(model.Stages, serializer);
			using (var diagnostics = DiagnosticRunner.Start<TEntity>(Connection, model))
			{
				IAsyncCursor<TResult> underlyingCursor;

				try
				{
					underlyingCursor = GetCollection().Aggregate(pipeline);
				}
				catch (Exception exception)
				{
					diagnostics.Error(exception);
					throw;
				}

				var hasFirstResult = false;
				while (underlyingCursor.MoveNext())
				{
					if (!hasFirstResult)
					{
						hasFirstResult = true;
						diagnostics.FirstReadResult<TResult>();
					}

					var resultBatch = underlyingCursor.Current;
					foreach (var item in resultBatch)
					{
						if (item is TEntity entityItem && (model.ResultTransformer == null || model.ResultTransformer.ReturnType == typeof(TEntity)))
						{
							EntityProcessors.ProcessEntity(entityItem, Connection);
						}

						yield return item;
					}
				}
			}
		}

		private async IAsyncEnumerable<TResult> ExecuteModelAsync<TResult>(AggregateExecutionModel model, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			var serializer = model.Serializer as IBsonSerializer<TResult>;
			var pipeline = PipelineDefinition<TEntity, TResult>.Create(model.Stages, serializer);

			using (var diagnostics = DiagnosticRunner.Start<TEntity>(Connection, model))
			{
				IAsyncCursor<TResult> underlyingCursor;

				try
				{
					underlyingCursor = await GetCollection().AggregateAsync(pipeline, cancellationToken: cancellationToken);
				}
				catch (Exception exception)
				{
					diagnostics.Error(exception);
					throw;
				}

				var hasFirstResult = false;
				while (await underlyingCursor.MoveNextAsync(cancellationToken))
				{
					if (!hasFirstResult)
					{
						hasFirstResult = true;
						diagnostics.FirstReadResult<TResult>();
					}

					var resultBatch = underlyingCursor.Current;
					foreach (var item in resultBatch)
					{
						if (item is TEntity entityItem &&
							(model.ResultTransformer == null ||
							model.ResultTransformer.ReturnType == typeof(ValueTask<TEntity>) ||
							model.ResultTransformer.ReturnType == typeof(Task<TEntity>)))
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
