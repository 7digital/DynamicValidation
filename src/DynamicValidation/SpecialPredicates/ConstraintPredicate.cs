using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace DynamicValidation.SpecialPredicates
{
	public class ConstraintPredicate : INamedPredicate
	{
		readonly Constraint constraint;

		public ConstraintPredicate(IResolveConstraint constraint)
		{
			this.constraint = constraint.Resolve();
		}

		public bool Matches(object actual, out string message)
		{
			if (constraint.Matches(actual))
			{
				message = null;
				return true;
			}

			var sw = new TextMessageWriter();
			constraint.WriteMessageTo(sw);
			message = sw.ToString();
			return false;
		}
	}
}