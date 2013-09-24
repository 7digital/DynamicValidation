namespace DynamicValidation
{
	/// <summary>
	/// Base interface of assertion predicates.
	/// </summary>
	public interface INamedPredicate
	{
		/// <summary>
		/// Given an actual, does it meet the predicate
		/// </summary>
		bool Matches(object actual, out string message);
	}
}