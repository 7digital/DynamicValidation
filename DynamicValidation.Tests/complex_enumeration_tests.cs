using System.Collections.Generic;
using NUnit.Framework;

namespace DynamicValidation.Tests
{
	[TestFixture, Explicit("Failing tests for upcoming features")]
	public class complex_enumeration_tests
	{
		readonly object subject = new X
		{
			container = new List<object>{
				new { a = true,  b = true},
				new { a = false, b = true},
				new { a = true,  b = true}
			},
			singleItem = new List<object>{
				new { a = true }
			},
			emptyItem = new List<object>()
		};

		[Test]
		[TestCase(0, true)]
		[TestCase(1, false)]
		[TestCase(2, true)]
		public void can_check_that_nth_item_validates(int n, bool a_value)
		{
			var result = Check.That(subject).container(n).a[Is.EqualTo(a_value)];

			Assert.That(result.Success, Is.True, string.Join(" ", result.Reasons));
		}

		[Test]
		public void all_child_validation_fails_correctly()
		{
			var result = Check.That(subject).container("all").a[Is.True];

			Assert.That(result.Success, Is.False);
			Assert.That(result.Reasons, Contains.Item("not all X.container.a matched Expected: True\nBut got: False"));
		}

		[Test]
		public void all_child_validation_passes_correctly()
		{
			var result = Check.That(subject).container("all").b[Is.True];

			Assert.That(result.Success, Is.True, string.Join(" ", result.Reasons));
		}

		[Test]
		public void any_child_validation_passes_correctly()
		{
			var result = Check.That(subject).container("any").a[Is.True];

			Assert.That(result.Success, Is.True);
		}

		[Test]
		public void any_child_validation_fails_correctly()
		{
			var result = Check.That(subject).container("any").b[Is.False];

			Assert.That(result.Success, Is.False);
			Assert.That(result.Reasons, Contains.Item("no X.container.b matched Expected: True\nBut got: False"));
		}

		[Test]
		public void single_child_validation_fails_on_multiple_items()
		{
			var result = Check.That(subject).container("single").a[Is.False];

			Assert.That(result.Success, Is.False);
			Assert.That(result.Reasons, Contains.Item("X.container has more than one item"));
		}

		[Test]
		public void single_child_validation_fails_on_no_items()
		{
			var result = Check.That(subject).emptyItem("single").a[Is.False];

			Assert.That(result.Success, Is.False);
			Assert.That(result.Reasons, Contains.Item("X.container has no items"));
		}

		[Test]
		public void single_child_validation_passes_with_one_item()
		{
			var result = Check.That(subject).singleItem("single").a[Is.True];

			Assert.That(result.Success, Is.True, string.Join(" ", result.Reasons));
		}
	}

	class X
	{
		public List<object> container;
		public List<object> singleItem;
		public List<object> emptyItem;
	}
}
