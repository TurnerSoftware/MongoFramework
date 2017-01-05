using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoFramework
{
	/// <summary>
	/// Instructs the MongoDb Driver to ignore extra elements (properties defined in the DB that aren't on the model)
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = true)]
	public class IgnoreExtraElementsAttribute : Attribute
	{
		/// <summary>
		/// Sets whether the value should be inherited by derived classes.
		/// </summary>
		public bool IgnoreInherited { get; set; }
	}
}
