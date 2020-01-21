using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MongoFramework.Infrastructure
{
	public class ShallowPropertyEqualityComparer<TObject> : IEqualityComparer<TObject>
	{
		private PropertyInfo[] Properties { get; }
		private static bool ImplementsIEquatable { get; } = typeof(TObject).GetInterfaces().Any(i => i == typeof(IEquatable<TObject>));

		public ShallowPropertyEqualityComparer()
		{
			if (!ImplementsIEquatable)
			{
				Properties = typeof(TObject).GetProperties(BindingFlags.Public | BindingFlags.Instance);
			}
		}
		public bool Equals(TObject x, TObject y)
		{
			if (object.Equals(x, default(TObject)) && object.Equals(y, default(TObject)))
			{
				return true;
			}
			else if (object.Equals(x, default(TObject)) || object.Equals(y, default(TObject)))
			{
				return false;
			}

			if (ImplementsIEquatable)
			{
				return (x as IEquatable<TObject>).Equals(y);
			}

			foreach (var property in Properties)
			{
				var xValue = property.GetValue(x);
				var yValue = property.GetValue(y);

				if (!Equals(xValue, yValue))
				{
					return false;
				}
			}

			return true;
		}

		public int GetHashCode(TObject obj)
		{
			if (obj is null)
			{
				throw new ArgumentNullException(nameof(obj));
			}

			if (ImplementsIEquatable)
			{
				return obj.GetHashCode();
			}

			var combinedHashCode = 1;
			foreach (var property in Properties)
			{
				var xValue = property.GetValue(obj);
				if (xValue != null)
				{
					var localHashCode = xValue.GetHashCode();
					combinedHashCode = (((localHashCode << 5) + localHashCode) ^ combinedHashCode);
				}
			}

			return combinedHashCode;
		}
	}
}
