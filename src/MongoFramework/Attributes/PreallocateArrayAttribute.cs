using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MongoFramework.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class PreallocateArrayAttribute : MutatePropertyAttribute
	{
		public int NumberOfItems { get; }

		public PreallocateArrayAttribute(int numberOfItems)
		{
			if (numberOfItems < 1)
			{
				throw new ArgumentException("The number of items to preallocate must be at least 1", nameof(numberOfItems));
			}

			NumberOfItems = numberOfItems;
		}

		public override void OnInsert(object target, PropertyInfo property)
		{
			var propertyType = property.PropertyType;
			if (!propertyType.IsGenericType)
			{
				throw new ArgumentException(
					"Unable to determine item type. Make sure the property type is compatible with `IEnumerable<YourType>`.",
					nameof(property)
				);
			}

			var genericTypeDefinition = propertyType.GetGenericTypeDefinition();
			if (genericTypeDefinition != typeof(IEnumerable<>) && !genericTypeDefinition.GetInterfaces()
				.Any(t => t.IsGenericType &&
						  t.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
			{
				throw new ArgumentException(
					"Property type is incompatible with preallocation. Make sure the property type is compatible with `IEnumerable<YourType>`",
					nameof(property)
				);
			}

			var itemType = propertyType.GetGenericArguments()[0];
			var generateMethod = GetType().GetMethod(nameof(GenerateItemArray), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(itemType);

			var initialValue = property.GetValue(target);
			var preallocatedValue = generateMethod.Invoke(this, new[] { initialValue });
			property.SetValue(target, preallocatedValue);
		}

#pragma warning disable CRR0026 // Unused member
		private IEnumerable<TItem> GenerateItemArray<TItem>(IEnumerable<TItem> existingElements)
		{
			if (existingElements == null)
			{
				existingElements = Enumerable.Empty<TItem>();
			}

			var itemsToAdd = NumberOfItems - existingElements.Count();
			if (itemsToAdd < 1)
			{
				return existingElements;
			}

			var itemInstance = Activator.CreateInstance<TItem>();
			var preallocatedItemArray = new TItem[itemsToAdd];

			for (int i = 0, l = itemsToAdd; i < l; i++)
			{
				preallocatedItemArray[i] = itemInstance;
			}

			return existingElements.Concat(preallocatedItemArray);
		}
#pragma warning restore CRR0026 // Unused member
	}
}
