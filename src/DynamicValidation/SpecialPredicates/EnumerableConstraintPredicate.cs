using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace DynamicValidation.SpecialPredicates
{
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