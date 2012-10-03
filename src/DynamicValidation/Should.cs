using System.Collections.Generic;
using System.Linq;
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
}
