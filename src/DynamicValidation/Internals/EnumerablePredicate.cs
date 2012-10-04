using System.Collections.Generic;

namespace DynamicValidation.Internals
{
	public class EnumerablePredicate : INamedPredicate
	{
		readonly INamedPredicate constraint;

		public EnumerablePredicate(INamedPredicate constraint)
		{
			this.constraint = constraint;
		}

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