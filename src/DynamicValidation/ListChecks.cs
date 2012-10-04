namespace DynamicValidation
{
	/// <summary>
	/// Constant strings for list strategies.
	/// You can use literal strings instead if you like!
	/// </summary>
	public class ListChecks
	{
		/// <summary> Every sub-path must validate correctly </summary>
		public const string All = "all";

		/// <summary> At least one sub-path must validate correctly </summary>
		public const string Any = "any";

		/// <summary> There must be exactly one sub-path and it must validate correctly </summary>
		public const string Single = "single";

		internal const string Index = "index"; // internal marker that user supplied an index.
	}
}
