using System.Collections.Generic;
using NUnit.Framework;

namespace DynamicValidation.Tests
{
	[TestFixture]
	public class common_predicate_tests
	{
		readonly object subject = new Outer {container = new List<string>{
			"one", "two", "three"
		}};

		[Test]
		public void exact_item_count()
		{
			var pass = Check.That(subject).container[Should.Have(3)];
			var fail = Check.That(subject).container[Should.Have(42)];

			Assert.That(pass.Success);
			Assert.That(fail.Success, Is.False);
			Assert.That(fail.Reasons, Contains.Item("Outer.container should have 42 items exactly"));
		}
		
		[Test]
		public void __should_be__checks_target_against_a_lambda()
		{
			// passing a lambda in a closure works around the dynamic issues.
			var pass = Check.That(subject).container[Should.Be(o=>o != null, "Was null, and that's bad!")];
			var fail = Check.That(subject).container[Should.Be(o=>o == null, "Was not null... unexpected!")];

			Assert.That(pass.Success);
			Assert.That(fail.Success, Is.False);
			Assert.That(fail.Reasons, Contains.Item("Outer.container Was not null... unexpected!"));
		}
		
		[Test]
		public void __should_all_be__checks_each_item_in_target_enumerable_against_a_lambda()
		{
			// passing a lambda in a closure works around the dynamic issues.
			var pass = Check.That(subject).container[Should.AllBe(o=>o is string, "All items should be strings")];
			var fail = Check.That(subject).container[Should.AllBe(o=>(string)o == "two", "Expected all items to be 'two'")];

			Assert.That(pass.Success);
			Assert.That(fail.Success, Is.False);
			Assert.That(fail.Reasons, Contains.Item("Outer.container Expected all items to be 'two'"));
		}
		
		
		[Test]
		public void minimum_item_count()
		{
			var pass = Check.That(subject).container[Should.HaveAtLeast(2)];
			var fail = Check.That(subject).container[Should.HaveAtLeast(42)];

			Assert.That(pass.Success);
			Assert.That(fail.Success, Is.False);
			Assert.That(fail.Reasons, Contains.Item("Outer.container should have at least 42 items"));
		}
		
		[Test]
		public void maximum_item_count()
		{
			var pass = Check.That(subject).container[Should.HaveLessThan(4)];
			var fail = Check.That(subject).container[Should.HaveLessThan(3)];

			Assert.That(pass.Success);
			Assert.That(fail.Success, Is.False);
			Assert.That(fail.Reasons, Contains.Item("Outer.container should have less than 3 items"));
		}

		[Test]
		public void can_check_each_item_of_a_list()
		{
			var result = Check.That(subject).container[Should.AllMatch(Should.NotBeNull)];

			Assert.That(result.Success, Is.True, string.Join(" ", result.Reasons));
		}
	}

	class Outer {
		public List<string> container;
	}
}
