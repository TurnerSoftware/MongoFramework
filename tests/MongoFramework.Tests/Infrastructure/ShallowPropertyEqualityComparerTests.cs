using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoFramework.Infrastructure;

namespace MongoFramework.Tests.Infrastructure
{
	[TestClass]
	public class ShallowPropertyEqualityComparerTests
	{
		class SlowPathTestClass
		{
			public string Test { get; set; }
			public int Number { get; set; }
		}

		class FastPathTestClass : IEquatable<FastPathTestClass>
		{
			public string Test { get; set; }
			public int Number { get; set; }

			public override bool Equals(object obj)
			{
				return Equals(obj as FastPathTestClass);
			}

			public override int GetHashCode()
			{
				var hashCode = 1;
				var testHashCode = Test?.GetHashCode() ?? 1;
				var numberHashCode = Number.GetHashCode();
				hashCode = (((testHashCode << 5) + testHashCode) ^ hashCode);
				hashCode = (((numberHashCode << 5) + numberHashCode) ^ hashCode);
				return hashCode;
			}

			public bool Equals(FastPathTestClass other)
			{
				if (other == null)
				{
					return false;
				}

				return object.Equals(Test, other.Test) && object.Equals(Number, other.Number);
			}
		}

		[TestMethod]
		public void SlowPathEqual()
		{
			var comparer = new ShallowPropertyEqualityComparer<SlowPathTestClass>();

			Assert.IsTrue(comparer.Equals(new SlowPathTestClass(), new SlowPathTestClass()));
			Assert.IsTrue(comparer.Equals(null, null));
			Assert.IsTrue(comparer.Equals(new SlowPathTestClass
			{
				Number = 5
			}, new SlowPathTestClass
			{
				Number = 5
			}));
			Assert.IsTrue(comparer.Equals(new SlowPathTestClass
			{
				Test = "ABC"
			}, new SlowPathTestClass
			{
				Test = "ABC"
			}));
			Assert.IsTrue(comparer.Equals(new SlowPathTestClass
			{
				Number = 1,
				Test = "Foo"
			}, new SlowPathTestClass
			{
				Number = 1,
				Test = "Foo"
			}));
		}
		[TestMethod]
		public void SlowPathNotEqual()
		{
			var comparer = new ShallowPropertyEqualityComparer<SlowPathTestClass>();

			Assert.IsFalse(comparer.Equals(new SlowPathTestClass(), null));
			Assert.IsFalse(comparer.Equals(null, new SlowPathTestClass()));
			Assert.IsFalse(comparer.Equals(new SlowPathTestClass
			{
				Number = 5
			}, new SlowPathTestClass
			{
				Number = 10
			}));
			Assert.IsFalse(comparer.Equals(new SlowPathTestClass
			{
				Test = "FOO"
			}, new SlowPathTestClass
			{
				Test = "BAR"
			}));
		}

		[TestMethod]
		public void FastPathEqual()
		{
			var comparer = new ShallowPropertyEqualityComparer<FastPathTestClass>();

			Assert.IsTrue(comparer.Equals(new FastPathTestClass(), new FastPathTestClass()));
			Assert.IsTrue(comparer.Equals(null, null));
			Assert.IsTrue(comparer.Equals(new FastPathTestClass
			{
				Number = 5
			}, new FastPathTestClass
			{
				Number = 5
			}));
			Assert.IsTrue(comparer.Equals(new FastPathTestClass
			{
				Test = "ABC"
			}, new FastPathTestClass
			{
				Test = "ABC"
			}));
			Assert.IsTrue(comparer.Equals(new FastPathTestClass
			{
				Number = 1,
				Test = "Foo"
			}, new FastPathTestClass
			{
				Number = 1,
				Test = "Foo"
			}));
		}
		[TestMethod]
		public void FastPathNotEqual()
		{
			var comparer = new ShallowPropertyEqualityComparer<FastPathTestClass>();

			Assert.IsFalse(comparer.Equals(new FastPathTestClass(), null));
			Assert.IsFalse(comparer.Equals(null, new FastPathTestClass()));
			Assert.IsFalse(comparer.Equals(new FastPathTestClass
			{
				Number = 5
			}, new FastPathTestClass
			{
				Number = 10
			}));
			Assert.IsFalse(comparer.Equals(new FastPathTestClass
			{
				Test = "FOO"
			}, new FastPathTestClass
			{
				Test = "BAR"
			}));
		}
	}
}
