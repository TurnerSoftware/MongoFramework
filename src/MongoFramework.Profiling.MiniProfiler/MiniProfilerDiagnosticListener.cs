﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Diagnostics;
using StackExchange.Profiling;

namespace MongoFramework.Profiling.MiniProfiler
{
	public class MiniProfilerDiagnosticListener : IDiagnosticListener
	{
		private static MethodInfo OnNextWriteCommandMethod { get; } = typeof(MiniProfilerDiagnosticListener)
			.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
			.Where(m => m.IsGenericMethod && m.Name == nameof(OnNextWriteCommand)).FirstOrDefault();
		private static MethodInfo OnNextIndexCommandMethod { get; } = typeof(MiniProfilerDiagnosticListener)
			.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
			.Where(m => m.IsGenericMethod && m.Name == nameof(OnNextIndexCommand)).FirstOrDefault();

		private ConcurrentDictionary<Guid, CustomTiming> Commands { get; } = new ConcurrentDictionary<Guid, CustomTiming>();

		public void OnCompleted() { /* OnCompleted is never called in MongoFramework as commands have a state */ }

		public void OnError(Exception error) => Trace.WriteLine(error);

		public void OnNext(DiagnosticCommand value)
		{
			if (StackExchange.Profiling.MiniProfiler.Current == null)
			{
				return;
			}

			if (value.CommandState == CommandState.Start)
			{
				if (value is ReadDiagnosticCommand readCommand)
				{
					OnNextReadCommand(readCommand);
				}
				else if (value is WriteDiagnosticCommandBase writeCommandBase)
				{
					OnNextWriteCommand(writeCommandBase);
				}
				else if (value is IndexDiagnosticCommandBase indexCommandBase)
				{
					OnNextIndexCommand(indexCommandBase);
				}
			}
			else if (Commands.TryRemove(value.CommandId, out var current))
			{
				if (value.CommandState == CommandState.FirstResult)
				{
					Commands[value.CommandId] = current;
					current.FirstFetchCompleted();
				}
				else
				{
					current.Errored = value.CommandState == CommandState.Error;
					current.Stop();
				}
			}
		}

		private void OnNextReadCommand(ReadDiagnosticCommand command)
		{
			Commands[command.CommandId] = StackExchange.Profiling.MiniProfiler.Current.CustomTiming("mongodb", command.Query, "Read");
		}

		private void OnNextWriteCommand(WriteDiagnosticCommandBase commandBase)
		{
			OnNextWriteCommandMethod.MakeGenericMethod(commandBase.EntityType).Invoke(this, new[] { commandBase });
		}
#pragma warning disable CRR0026 // Unused member
		private void OnNextWriteCommand<TEntity>(WriteDiagnosticCommand<TEntity> command)
		{
			var queryList = command.WriteModel.GroupBy(w => w.ModelType)
					.Select(g => g.Key.ToString() + "\n" + string.Join("\n", g.Select(w => GetWriteModelAsString(w))));
			var writeModelString = string.Join("; ", queryList);
			Commands[command.CommandId] = StackExchange.Profiling.MiniProfiler.Current.CustomTiming("mongodb", writeModelString, "Write");
		}
#pragma warning restore CRR0026 // Unused member
		private string GetWriteModelAsString<TEntity>(WriteModel<TEntity> writeModel)
		{
			if (writeModel is InsertOneModel<TEntity> insertModel)
			{
				return new BsonDocument
				{
					{ "Document", insertModel.Document.ToBsonDocument() }
				}.ToString();
			}
			else if (writeModel is UpdateOneModel<TEntity> updateModel)
			{
				var serializer = BsonSerializer.LookupSerializer<TEntity>();
				return new BsonDocument
				{
					{ "Filter", updateModel.Filter.Render(serializer, BsonSerializer.SerializerRegistry) },
					{ "Update", updateModel.Update.Render(serializer, BsonSerializer.SerializerRegistry) }
				}.ToString();
			}
			else if (writeModel is DeleteOneModel<TEntity> deleteModel)
			{
				var serializer = BsonSerializer.LookupSerializer<TEntity>();
				return new BsonDocument
				{
					{ "Filter", deleteModel.Filter.Render(serializer, BsonSerializer.SerializerRegistry) }
				}.ToString();
			}
			else
			{
				return $"Can't render {writeModel.ModelType} to a string";
			}
		}

		private void OnNextIndexCommand(IndexDiagnosticCommandBase commandBase)
		{
			OnNextIndexCommandMethod.MakeGenericMethod(commandBase.EntityType).Invoke(this, new[] { commandBase });
		}
#pragma warning disable CRR0026 // Unused member
		private void OnNextIndexCommand<TEntity>(IndexDiagnosticCommand<TEntity> command)
		{
			var queryList = command.IndexModel.GroupBy(w => w.Options.Name)
					.Select(g => g.Key.ToString() + "\n" + string.Join("\n", g.Select(w => GetIndexModelAsString(w))));
			var indexModelString = string.Join("; ", queryList);
			Commands[command.CommandId] = StackExchange.Profiling.MiniProfiler.Current.CustomTiming("mongodb", indexModelString, "Index");
		}
#pragma warning restore CRR0026 // Unused member
		private string GetIndexModelAsString<TEntity>(CreateIndexModel<TEntity> indexModel)
		{
			var serializer = BsonSerializer.LookupSerializer<TEntity>();
			var indexOptions = indexModel.Options;
			return new BsonDocument
			{
				{ "Keys", indexModel.Keys.Render(serializer, BsonSerializer.SerializerRegistry) },
				{ "Options", new { indexOptions.Name, indexOptions.Unique, indexOptions.Background }.ToBsonDocument() }
			}.ToString();
		}
	}
}
