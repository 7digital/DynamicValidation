using System;
using System.Collections.Generic;
using System.Linq;
using DynamicValidation.SpecialPredicates;

namespace DynamicValidation
{
	/// <summary>
	/// Extensions to `Should`
	/// </summary>
	public static class ShouldExtensions
	{
		/// <summary>
		/// Succeeds if either left or right succeeds.
		/// If both fail, merges and flattens messages.
		/// </summary>
		public static INamedPredicate Or(this INamedPredicate left, INamedPredicate right)
		{
			return new NamedPredicate(
				o =>
				{
					string dummy;
					return left.Matches(o, out dummy) || right.Matches(o, out dummy);
				},
				o =>
				{
					string leftMsg, rightMsg;
					left.Matches(o, out leftMsg);
					right.Matches(o, out rightMsg);
					return leftMsg + ", " + rightMsg;
				}
				);
		}


		/// <summary>
		/// Applies only to enumerable nodes.
		/// The source predicate will only be applied to nodes where the filter returns true.
		/// </summary>
		/// <param name="source">A predicate, like `Should.Have(3)`</param>
		/// <param name="filter">A filter, like `thing => thing.Attribute = "target"`</param>
		public static INamedPredicate Where(this INamedPredicate source, Func<dynamic, bool> filter)
		{
			return new NamedPredicate(
						o => { 
							var filtered = ((IEnumerable<object>)o).Where(filter);
							string dummy;
							return source.Matches(filtered, out dummy);
						},
						o =>
						{
							var filtered = ((IEnumerable<object>)o).Where(filter);
							string message;
							source.Matches(filtered, out message);
							return message;
						}
				);

		}
	}
}