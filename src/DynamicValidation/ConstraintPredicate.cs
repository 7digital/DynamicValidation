using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace DynamicValidation
{
	public class ConstraintPredicate : INamedPredicate
	{
		readonly IResolveConstraint constraint;

		public ConstraintPredicate(IResolveConstraint constraint)
		{
			this.constraint = constraint;
		}

		public bool Matches(object actual, out string message)
		{
			var check = constraint.Resolve();

			if (check.Matches(actual))
			{
				message = null;
				return true;
			}

			var sw = new TextMessageWriter();
			check.WriteMessageTo(sw);
			message = sw.ToString();
			return false;
		}
	}
}