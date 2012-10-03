using System;

namespace DynamicValidation
{
	public class NamedPredicate : INamedPredicate
	{
		readonly Func<object, bool> predicate;
		readonly string failureMessage;

		public NamedPredicate(Func<object, bool> predicate, string failureMessage)
		{
			this.predicate = predicate;
			this.failureMessage = failureMessage;
		}

		public bool Matches(object actual, out string message)
		{
			message = failureMessage;
			return predicate(actual);
		}
	}
}