using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

namespace MongoFramework.Linq
{
	// From the .NET Runtime: https://github.com/dotnet/runtime/blob/6072e4d3a7a2a1493f514cdf4be75a3d56580e84/src/libraries/System.Linq.Queryable/src/System/Linq/CachedReflection.cs
	// Licensed to the .NET Foundation under one or more agreements.
	// The .NET Foundation licenses this file to you under the MIT license.
	internal static class QueryableMethods
	{
		private static MethodInfo? s_All_TSource_2;

		public static MethodInfo All_TSource_2(Type TSource) =>
			 (s_All_TSource_2 ??
			 (s_All_TSource_2 = new Func<IQueryable<object>, Expression<Func<object, bool>>, bool>(Queryable.All).GetMethodInfo().GetGenericMethodDefinition()))
			  .MakeGenericMethod(TSource);

		private static MethodInfo? s_Any_TSource_1;

		public static MethodInfo Any_TSource_1(Type TSource) =>
			 (s_Any_TSource_1 ??
			 (s_Any_TSource_1 = new Func<IQueryable<object>, bool>(Queryable.Any).GetMethodInfo().GetGenericMethodDefinition()))
			  .MakeGenericMethod(TSource);

		private static MethodInfo? s_Any_TSource_2;

		public static MethodInfo Any_TSource_2(Type TSource) =>
			 (s_Any_TSource_2 ??
			 (s_Any_TSource_2 = new Func<IQueryable<object>, Expression<Func<object, bool>>, bool>(Queryable.Any).GetMethodInfo().GetGenericMethodDefinition()))
			  .MakeGenericMethod(TSource);

		private static MethodInfo? s_Count_TSource_1;

		public static MethodInfo Count_TSource_1(Type TSource) =>
			 (s_Count_TSource_1 ??
			 (s_Count_TSource_1 = new Func<IQueryable<object>, int>(Queryable.Count).GetMethodInfo().GetGenericMethodDefinition()))
			  .MakeGenericMethod(TSource);

		private static MethodInfo? s_Count_TSource_2;

		public static MethodInfo Count_TSource_2(Type TSource) =>
			 (s_Count_TSource_2 ??
			 (s_Count_TSource_2 = new Func<IQueryable<object>, Expression<Func<object, bool>>, int>(Queryable.Count).GetMethodInfo().GetGenericMethodDefinition()))
			  .MakeGenericMethod(TSource);

		private static MethodInfo? s_First_TSource_1;

		public static MethodInfo First_TSource_1(Type TSource) =>
			 (s_First_TSource_1 ??
			 (s_First_TSource_1 = new Func<IQueryable<object>, object>(Queryable.First).GetMethodInfo().GetGenericMethodDefinition()))
			  .MakeGenericMethod(TSource);

		private static MethodInfo? s_First_TSource_2;

		public static MethodInfo First_TSource_2(Type TSource) =>
			 (s_First_TSource_2 ??
			 (s_First_TSource_2 = new Func<IQueryable<object>, Expression<Func<object, bool>>, object>(Queryable.First).GetMethodInfo().GetGenericMethodDefinition()))
			  .MakeGenericMethod(TSource);

		private static MethodInfo? s_FirstOrDefault_TSource_1;

		public static MethodInfo FirstOrDefault_TSource_1(Type TSource) =>
			 (s_FirstOrDefault_TSource_1 ??
			 (s_FirstOrDefault_TSource_1 = new Func<IQueryable<object>, object>(Queryable.FirstOrDefault).GetMethodInfo().GetGenericMethodDefinition()))
			  .MakeGenericMethod(TSource);

		private static MethodInfo? s_FirstOrDefault_TSource_2;

		public static MethodInfo FirstOrDefault_TSource_2(Type TSource) =>
			 (s_FirstOrDefault_TSource_2 ??
			 (s_FirstOrDefault_TSource_2 = new Func<IQueryable<object>, Expression<Func<object, bool>>, object>(Queryable.FirstOrDefault).GetMethodInfo().GetGenericMethodDefinition()))
			  .MakeGenericMethod(TSource);

		private static MethodInfo? s_Single_TSource_1;

		public static MethodInfo Single_TSource_1(Type TSource) =>
			 (s_Single_TSource_1 ??
			 (s_Single_TSource_1 = new Func<IQueryable<object>, object>(Queryable.Single).GetMethodInfo().GetGenericMethodDefinition()))
			  .MakeGenericMethod(TSource);

		private static MethodInfo? s_Single_TSource_2;

		public static MethodInfo Single_TSource_2(Type TSource) =>
			 (s_Single_TSource_2 ??
			 (s_Single_TSource_2 = new Func<IQueryable<object>, Expression<Func<object, bool>>, object>(Queryable.Single).GetMethodInfo().GetGenericMethodDefinition()))
			  .MakeGenericMethod(TSource);

		private static MethodInfo? s_SingleOrDefault_TSource_1;

		public static MethodInfo SingleOrDefault_TSource_1(Type TSource) =>
			 (s_SingleOrDefault_TSource_1 ??
			 (s_SingleOrDefault_TSource_1 = new Func<IQueryable<object>, object>(Queryable.SingleOrDefault).GetMethodInfo().GetGenericMethodDefinition()))
			  .MakeGenericMethod(TSource);

		private static MethodInfo? s_SingleOrDefault_TSource_2;

		public static MethodInfo SingleOrDefault_TSource_2(Type TSource) =>
			 (s_SingleOrDefault_TSource_2 ??
			 (s_SingleOrDefault_TSource_2 = new Func<IQueryable<object>, Expression<Func<object, bool>>, object>(Queryable.SingleOrDefault).GetMethodInfo().GetGenericMethodDefinition()))
			  .MakeGenericMethod(TSource);
	}
}

#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.