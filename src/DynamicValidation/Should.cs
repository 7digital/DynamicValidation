using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace DynamicValidation
{
	/// <summary>
	/// Some common assertions for use in dynamic validation
	/// </summary>
	public class Should
	{
		public static INamedPredicate Have(int n)
		{
			return new NamedPredicate(
						o => ((IEnumerable<object>)o).Count() == n,
						"Should have " + n + " items exactly"
						);
		}

		public static INamedPredicate HaveAtLeast(int n)
		{
			return new NamedPredicate(
						o => ((IEnumerable<object>)o).Count() >= n,
						"Should have at least " + n + " items"
						);
		}

		public static INamedPredicate HaveLessThan(int n)
		{
			return new NamedPredicate(
						o => ((IEnumerable<object>)o).Count() < n,
						"Should have less than " + n + " items"
						);
		}

		public static INamedPredicate AllMatch(IResolveConstraint constraint)
		{
			return new EnumerableConstraintPredicate(constraint);
		}
	}

	public class EnumerableConstraintPredicate : INamedPredicate
	{
		readonly IResolveConstraint constraint;

		public EnumerableConstraintPredicate(IResolveConstraint constraint)
		{
			this.constraint = constraint;
		}

		public bool Matches(object actual, out string message)
		{
			message = "";
			var result = true;
			var check = constraint.Resolve();
			foreach (var obj in ((IEnumerable<object>)actual))
			{

				if (check.Matches(obj))
				{
					continue;
				}

				result = false;
				var sw = new TextMessageWriter();
				check.WriteMessageTo(sw);
				message += sw+"\r\n";
			}
			return result;
		}
	}
}
