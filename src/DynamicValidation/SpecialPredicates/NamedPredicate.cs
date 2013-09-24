using System;

namespace DynamicValidation.SpecialPredicates
{
	/// <summary>
	/// Basic predicate container
	/// </summary>
	public class NamedPredicate : INamedPredicate
	{
		readonly Func<object, bool> predicate;
		readonly Func<object,string> failureMessage;

		/// <summary>
		/// Create a predicate with a predicate function and a potential failure message function
		/// </summary>
		public NamedPredicate(Func<object, bool> predicate, Func<object,string> failureMessage)
		{
			this.predicate = predicate;
			this.failureMessage = failureMessage;
		}
		/// <summary>
		/// Create a predicate with a predicate function and a static failure message
		/// </summary>
		public NamedPredicate(Func<object, bool> predicate, string failureMessage)
		{
			this.predicate = predicate;
			this.failureMessage = o=>failureMessage;
		}

		/// <summary>
		/// Given an actual, does it meet the predicate
		/// </summary>
		public bool Matches(object actual, out string message)
		{
			message = failureMessage(actual);
			return predicate(actual);
		}
	}
}