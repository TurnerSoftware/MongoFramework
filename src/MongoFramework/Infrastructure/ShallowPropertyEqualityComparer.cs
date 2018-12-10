using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MongoFramework.Infrastructure
{
	public class ShallowPropertyEqualityComparer<TObject> : IEqualityComparer<TObject>
	{
		private PropertyInfo[] Properties { get; }
		private bool ImplementsIEquatable { get; }

		public ShallowPropertyEqualityComparer()
		{
			if (typeof(TObject).GetInterfaces().Any(i => i == typeof(IEquatable<TObject>)))
			{
				ImplementsIEquatable = true;
			}
			else
			{
				Properties = typeof(TObject).GetProperties(BindingFlags.Public | BindingFlags.Instance);
			}
		}

		public bool Equals(TObject x, TObject y)
		{
			if (x == null && y == null)
			{
				return true;
			}
			else if (x == null || y == null)
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

				if (!object.Equals(xValue, yValue))
				{
					return false;
				}
			}

			return true;
		}

		public int GetHashCode(TObject obj)
		{
			if (obj == null)
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
