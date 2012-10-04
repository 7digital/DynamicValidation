using System;
using System.Collections.Generic;
using System.Linq;
using DynamicValidation.Internals;
using DynamicValidation.SpecialPredicates;

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
						o => "should have " + n + " items exactly"
						);
		}

		public static INamedPredicate HaveAtLeast(int n)
		{
			return new NamedPredicate(
						o => ((IEnumerable<object>)o).Count() >= n,
						o => "should have at least " + n + " items"
						);
		}

		public static INamedPredicate HaveLessThan(int n)
		{
			return new NamedPredicate(
						o => ((IEnumerable<object>)o).Count() < n,
						o => "should have less than " + n + " items"
						);
		}

		public static INamedPredicate Be(Func<object, bool> pred, string message)
		{
			return new NamedPredicate(pred, o => message);
		}
		public static INamedPredicate Be<T>()
		{
			return new NamedPredicate(
				o => o is T,
				o => "expected " + typeof(T).Name + " but got " +
					((o == null) ? "null" : (o.GetType().Name)));
		}

		public static INamedPredicate AllMatch(INamedPredicate constraint)
		{
			return new EnumerablePredicate(constraint);
		}

		public static INamedPredicate AllBe(Func<object, bool> predicate, string message)
		{
			return new NamedPredicate(
						o => ((IEnumerable<object>)o).All(predicate),
						o => message
						);
		}

		public static INamedPredicate NotBeNull
		{
			get
			{
				return new NamedPredicate(
					  o => o != null,
					  o => "was null"
					  );
			}
		}

		public static object BeTrue
		{
			get
			{
				return new NamedPredicate(
					  o => (o as bool?) == true,
					  o => "expected True but got "+o
					  );
			}
		}

		public static object BeFalse
		{
			get
			{
				return new NamedPredicate(
					  o => (o as bool?) == false,
					  o => "expected False but got "+o
					  );
			}
		}

		public static object NotBeEmpty
		{
			get
			{
				return new NamedPredicate(
					  o => ! string.IsNullOrEmpty(o as string),
					  o => (o is string) ? "was empty" : "was not a string"
					  );
			}
		}

		public static object BeEmpty
		{
			get
			{
				return new NamedPredicate(
					  o => string.IsNullOrEmpty(o as string),
					  o => (o is string) ? ("was " + o) : "was not a string"
					  );
			}
		}
		
		public static INamedPredicate BeNull
		{
			get
			{
				return new NamedPredicate(
					  o => o == null,
					  o => "was not null"
					  );
			}
		}

		public static INamedPredicate Equal(object aValue)
		{
			return new NamedPredicate(
				o => o.Equals(aValue),
				o => "was not equal to "+aValue
				);
		}

		public static INamedPredicate Contain(string substring)
		{
			return new NamedPredicate(
					  o => ((o as string) != null) && ((string)o).Contains(substring),
					  o => (o is string) ? "did not contain \""+substring+"\"" : "was not a string"
					  );
		}

		public static INamedPredicate EqualOneOf(IEnumerable<object> acceptableValues)
		{
			if (acceptableValues == null) throw new ArgumentException("null values passed to Should.EqualOneOf");
			return new NamedPredicate(
					  acceptableValues.Contains,
					  o => "got \""+o+"\" which is not an acceptable value"
					  );
		}
	}
}
