using System;
using System.Collections.Generic;
using System.Linq;
using DynamicValidation.Internals;
using DynamicValidation.Reflection;
using DynamicValidation.SpecialPredicates;

// ReSharper disable PartialTypeWithSinglePart, RedundantNameQualifier
namespace DynamicValidation
{
	/// <summary>
	/// Some common assertions for use in dynamic validation
	/// </summary>
	public partial class Should
	{
		/// <summary>
		/// Checks that the enumerable has exactly the given number of children
		/// </summary>
		public static INamedPredicate Have(int n)
		{
			return new NamedPredicate(
						o => ((IEnumerable<object>)o).Count() == n,
						o =>
						{
							var c = ((IEnumerable<object>)o).Count();
							return c < 1 ? "has no items" : ("should have " + n + " items exactly, but had " + c);
						}
				);
		}

		/// <summary>
		/// An enumeration should have at least this many items
		/// </summary>
		public static INamedPredicate HaveAtLeast(int n)
		{
			return new NamedPredicate(
						o => ((IEnumerable<object>)o).Count() >= n,
						o => "should have at least " + n + " items"
						);
		}

		/// <summary>
		/// An enumeration should have less than this many items.
		/// </summary>
		public static INamedPredicate HaveLessThan(int n)
		{
			return new NamedPredicate(
						o => ((IEnumerable<object>)o).Count() < n,
						o => "should have less than " + n + " items"
						);
		}

		/// <summary>
		/// Should match a predicate function
		/// </summary>
		public static INamedPredicate Be(Func<object, bool> pred, string message)
		{
			return new NamedPredicate(pred, o => message);
		}

		/// <summary>
		/// Should be of the specified type
		/// </summary>
		public static INamedPredicate Be<T>()
		{
			return new NamedPredicate(
				o => o is T,
				o => "expected " + typeof(T).Name + " but got " +
					((o == null) ? "null" : (o.GetType().Name)));
		}

		/// <summary>
		/// All items in an enumerable should match the predicate
		/// </summary>
		public static INamedPredicate AllMatch(INamedPredicate constraint)
		{
			return new EnumerablePredicate(constraint);
		}

		/// <summary>
		/// All items in an enumerable should match the predicate function
		/// </summary>
		public static INamedPredicate AllBe(Func<object, bool> predicate, string message)
		{
			return new NamedPredicate(
						o => ((IEnumerable<object>)o).All(predicate),
						o => message
						);
		}
		
		/// <summary>
		/// No items in an enumerable should match the predicate function
		/// </summary>
		public static INamedPredicate HaveNone(Func<object, bool> predicate, string message)
		{
			return new NamedPredicate(
						o => !((IEnumerable<object>)o).Any(predicate),
						o => message
						);
		}

		/// <summary>
		/// Item should not be null
		/// </summary>
		public static INamedPredicate NotBeNull
		{
			get
			{
				return new NamedPredicate(
					  o => o != null,
					  o => "is null"
					  );
			}
		}

		/// <summary>
		/// Item should be boolean and true
		/// </summary>
		public static INamedPredicate BeTrue
		{
			get
			{
				return new NamedPredicate(
					  o => (o as bool?) == true,
					  o => "expected True but got " + o
					  );
			}
		}
		/// <summary> Always succeeds </summary>
		public static INamedPredicate BeAnything
		{
			get
			{
				return new NamedPredicate(
					  o => true,
					  o => ""
					  );
			}
		}

		/// <summary> Allows anything but bool == true </summary>
		public static INamedPredicate NotBeTrue
		{
			get
			{
				return new NamedPredicate(
					  o => (o as bool?) != true,
					  o => "got True where not allowed"
					  );
			}
		}

		/// <summary>
		/// Should be boolean and false
		/// </summary>
		public static INamedPredicate BeFalse
		{
			get
			{
				return new NamedPredicate(
					  o => (o as bool?) == false,
					  o => "expected False but got " + o
					  );
			}
		}

		/// <summary> Allows anything but bool == false </summary>
		public static INamedPredicate NotBeFalse
		{
			get
			{
				return new NamedPredicate(
					  o => (o as bool?) != false,
					  o => "got True where not allowed"
					  );
			}
		}

		/// <summary>
		/// Should be a non-empty string
		/// </summary>
		public static INamedPredicate NotBeEmpty
		{
			get
			{
				return new NamedPredicate(
					  o => !string.IsNullOrEmpty(o as string),
					  o =>
					  {
						  if (o == null) return "is missing";
						  return (o is string) ? "is empty" : "is not a string";
					  }
					);
			}
		}

		/// <summary>
		/// Should be an empty string
		/// </summary>
		public static INamedPredicate BeEmpty
		{
			get
			{
				return new NamedPredicate(
					  o => string.IsNullOrEmpty(o as string),
					  o => (o is string) ? ("should be empty but is \"" + o + "\"") : "is not a string"
					  );
			}
		}

		/// <summary>
		/// Should be null
		/// </summary>
		public static INamedPredicate BeNull
		{
			get
			{
				return new NamedPredicate(
					  o => o == null,
					  o => "is not null"
					  );
			}
		}

		/// <summary>
		/// Should be equal to the given object
		/// </summary>
		public static INamedPredicate Equal(object aValue)
		{
			return new NamedPredicate(
				o => Equals(o, aValue),
				o => "is not equal to " + aValue
				);
		}

		/// <summary>
		/// Should be a string and contain the given substring
		/// </summary>
		public static INamedPredicate Contain(string substring)
		{
			return new NamedPredicate(
					  o => ((o as string) != null) && ((string)o).Contains(substring),
					  o => (o is string) ? "did not contain \"" + substring + "\"" : "is not a string"
					  );
		}

		/// <summary>
		/// Should be equal to at least one of the given values
		/// </summary>
		public static INamedPredicate EqualOneOf(IEnumerable<object> acceptableValues)
		{
			if (acceptableValues == null) throw new ArgumentException("null values passed to Should.EqualOneOf");
			return new NamedPredicate(
					  acceptableValues.Contains,
					  o => "got \"" + o + "\" which is not an acceptable value"
					  );
		}

		/// <summary>
		/// Should not equal any of the given values
		/// </summary>
		public static INamedPredicate NotEqualOneOf(IEnumerable<object> unacceptableValues)
		{
			if (unacceptableValues == null) throw new ArgumentException("null values passed to Should.NotEqualOneOf");
			return new NamedPredicate(
					  o => !unacceptableValues.Contains(o),
					  o => "got \"" + o + "\" which is not an acceptable value"
					  );
		}

		/// <summary>
		/// Checks that the type has the named member, and that it's not null
		/// </summary>
		public static INamedPredicate HaveMember(string memberName)
		{
			return new NamedPredicate(
					  o => (o != null) && (o.GetSafe(memberName) != null),
					  o => (o == null) ? "is null" : "did not contain member \"" + memberName + "\""
					  );
		}
		/// <summary>
		/// Checks that the type does not have the named member, or that it's null
		/// </summary>
		public static INamedPredicate NotHaveMember(string memberName)
		{
			return new NamedPredicate(
					  o => (o != null) && (o.GetSafe(memberName) == null),
					  o => (o == null) ? "is null" : "contained unexpected member \"" + memberName + "\""
					  );
		}

		/// <summary>
		/// Should be a date and earlier than the given date.
		/// </summary>
		public static INamedPredicate BeBefore(DateTime date)
		{
			return new NamedPredicate(o => DateIsBefore(o, date), "should be before " + date);
		}

		/// <summary>
		/// Should be a date and after the given date
		/// </summary>
		public static INamedPredicate BeAfter(DateTime date)
		{
			return new NamedPredicate(o => DateIsAfter(o, date), "should be after " + date);
		}

		/// <summary>
		/// Given two paths, they should both be dates,
		/// and the left-side should be earlier than the right
		/// </summary>
		public static INamedPredicate BeBefore(dynamic checkPath)
		{
			Check.Result result = checkPath[Should.BeAnything];
			var date = result.ValueChecked as DateTime?;
			var message = result.Path;

			if (date == null) return new NamedPredicate(o => false, "expected a date");
			return new NamedPredicate(o => DateIsBefore(o, date.Value), "should be before " + message);
		}
		
		/// <summary>
		/// Given two paths, they should both be dates,
		/// and the left-side should be either null or earlier than the right
		/// </summary>
		public static INamedPredicate BeNullOrBefore(dynamic checkPath)
		{
			Check.Result result = checkPath[Should.BeAnything];
			var date = result.ValueChecked as DateTime?;
			var message = result.Path;

			return new NamedPredicate(o => DateIsNullOrBefore(o, date), "should be null or before " + message);
		}

		/// <summary>
		/// Given two paths, they should both be dates,
		/// and the left-side should be later than the right
		/// </summary>
		public static INamedPredicate BeAfter(dynamic checkPath)
		{
			Check.Result result = checkPath[Should.BeAnything];
			var date = result.ValueChecked as DateTime?;
			var message = result.Path;

			if (date == null) return new NamedPredicate(o => false, "expected a date");
			return new NamedPredicate(o => DateIsAfter(o, date.Value), "should be after " + message);
		}
		
		
		/// <summary>
		/// Given two paths, they should both be dates,
		/// and the left-side should be either null or later than the right
		/// </summary>
		public static INamedPredicate BeNullOrAfter(dynamic checkPath)
		{
			Check.Result result = checkPath[Should.BeAnything];
			var date = result.ValueChecked as DateTime?;
			var message = result.Path;

			return new NamedPredicate(o => DateIsNullOrAfter(o, date), "should be null or after " + message);
		}

		static bool DateIsNullOrAfter(object o, DateTime? targetDate)
		{
			if (o == null) return true;
			var date = (DateTime)o;
			return date > targetDate;
		}

		static bool DateIsNullOrBefore(object o, DateTime? targetDate)
		{
			if (o == null) return true;
			var date = (DateTime)o;
			return date < targetDate;
		}

		static bool DateIsBefore(object o, DateTime targetDate)
		{
			var date = (DateTime)o;
			return date < targetDate;
		}

		static bool DateIsAfter(object o, DateTime targetDate)
		{
			var date = (DateTime)o;
			return date > targetDate;
		}

	}
}
