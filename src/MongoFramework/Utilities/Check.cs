// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// https://github.com/dotnet/efcore/blob/main/src/Shared/Check.cs

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MongoFramework.Utilities
{
	[DebuggerStepThrough]
	public static class Check
	{
		public static T NotNull<T>(T value, string parameterName)
		{
			if (value is null)
			{
				NotEmpty(parameterName, nameof(parameterName));
				throw new ArgumentNullException(parameterName);
			}

			return value;
		}

		public static IReadOnlyList<T> NotEmpty<T>(IReadOnlyList<T> value, string parameterName)
		{
			NotNull(value, parameterName);

			if (value.Count == 0)
			{
				NotEmpty(parameterName, nameof(parameterName));

				throw new ArgumentException($"The collection argument '{parameterName}' must contain at least one element.");
			}

			return value;
		}

		public static string NotEmpty(string value, string parameterName)
		{
			Exception e = null;
			if (value is null)
			{
				e = new ArgumentNullException(parameterName);
			}
			else if (value.Trim().Length == 0)
			{
				e = new ArgumentException($"The string argument '{parameterName}' cannot be empty.");
			}

			if (e != null)
			{
				NotEmpty(parameterName, nameof(parameterName));

				throw e;
			}

			return value;
		}

		public static string NullButNotEmpty(string value, string parameterName)
		{
			if (!(value is null)
				&& value.Length == 0)
			{
				NotEmpty(parameterName, nameof(parameterName));

				throw new ArgumentException("The string argument '{argumentName}' cannot be empty.");
			}

			return value;
		}

		public static IReadOnlyList<T> HasNoNulls<T>(IReadOnlyList<T> value, string parameterName)
			where T : class
		{
			NotNull(value, parameterName);

			if (value.Any(e => e == null))
			{
				NotEmpty(parameterName, nameof(parameterName));

				throw new ArgumentException(parameterName);
			}

			return value;
		}

		public static IReadOnlyList<string> HasNoEmptyElements(
			IReadOnlyList<string> value,
			string parameterName)
		{
			NotNull(value, parameterName);

			if (value.Any(s => string.IsNullOrWhiteSpace(s)))
			{
				NotEmpty(parameterName, nameof(parameterName));

				throw new ArgumentException($"The collection argument '{parameterName}' must not contain any empty elements.");
			}

			return value;
		}

		[Conditional("DEBUG")]
		public static void DebugAssert(bool condition, string message)
		{
			if (!condition)
			{
				throw new Exception($"Check.DebugAssert failed: {message}");
			}
		}
	}
}
