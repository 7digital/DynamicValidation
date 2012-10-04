using System;

namespace DynamicValidation.SpecialPredicates
{
	public class NamedPredicate : INamedPredicate
	{
		readonly Func<object, bool> predicate;
		readonly Func<object,string> failureMessage;

		public NamedPredicate(Func<object, bool> predicate, Func<object,string> failureMessage)
		{
			this.predicate = predicate;
			this.failureMessage = failureMessage;
		}
		public NamedPredicate(Func<object, bool> predicate, string failureMessage)
		{
			this.predicate = predicate;
			this.failureMessage = o=>failureMessage;
		}

		public bool Matches(object actual, out string message)
		{
			message = failureMessage(actual);
			return predicate(actual);
		}
	}
}