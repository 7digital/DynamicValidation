using System.Collections.Generic;

namespace DynamicValidation.Internals
{
	/// <summary>
	/// Enumeration predicates
	/// </summary>
	public class EnumerablePredicate : INamedPredicate
	{
		readonly INamedPredicate constraint;

		/// <summary>
		/// Appli a predicate to an enumerable
		/// </summary>
		public EnumerablePredicate(INamedPredicate constraint)
		{
			this.constraint = constraint;
		}

		/// <summary>
		/// Given an actual, does it meet the predicate
		/// </summary>
		public bool Matches(object actual, out string message)
		{
			var messages = new List<string>();
			var result = true;
			foreach (var obj in ((IEnumerable<object>)actual))
			{
				string temp;
				if (constraint.Matches(obj, out temp))
				{
					continue;
				}

				result = false;
				messages.Add(temp);
			}
			message = string.Join(", ", messages);
			return result;
		}
	}
}