// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// https://github.com/dotnet/efcore/blob/main/test/EFCore.Tests/Utilities/CheckTest.cs

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Utilities;

namespace MongoFramework.Tests.Utilities
{
	[TestClass]
	public class CheckTest
	{
		[TestMethod]
		public void Not_null_throws_when_arg_is_null()
		{
			Assert.ThrowsException<ArgumentNullException>(() => Check.NotNull<string>(null, "foo"));
		}

		[TestMethod]
		public void Not_null_throws_when_arg_name_empty()
		{
			Assert.ThrowsException<ArgumentException>(() => Check.NotNull(null as object, string.Empty));
		}

		[TestMethod]
		public void Not_empty_throws_when_empty()
		{
			Assert.ThrowsException<ArgumentException>(() => Check.NotEmpty("", string.Empty));
		}

		[TestMethod]
		public void Not_empty_throws_when_whitespace()
		{
			Assert.ThrowsException<ArgumentException>(() => Check.NotEmpty(" ", string.Empty));
		}

		[TestMethod]
		public void Not_empty_throws_when_parameter_name_null()
		{
			Assert.ThrowsException<ArgumentNullException>(() => Check.NotEmpty(null, null));
		}

		[TestMethod]
		public void Generic_Not_empty_throws_when_arg_is_empty()
		{
			Assert.ThrowsException<ArgumentException>(() => Check.NotEmpty(Array.Empty<string>(), "foo"));
		}

		[TestMethod]
		public void Generic_Not_empty_throws_when_arg_is_null()
		{
			Assert.ThrowsException<ArgumentNullException>(() => Check.NotEmpty<object>(null, "foo"));
		}

		[TestMethod]
		public void Generic_Not_empty_throws_when_arg_name_empty()
		{
			Assert.ThrowsException<ArgumentException>(() => Check.NotEmpty(null, string.Empty));
		}
	}
}
