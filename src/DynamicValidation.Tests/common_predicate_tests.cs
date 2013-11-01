using System.Collections.Generic;
using NUnit.Framework;

namespace DynamicValidation.Tests
{
	[TestFixture]
	public class common_predicate_tests
	{
		static readonly object aNullThing = null;
		readonly object subject = new Outer
		{
			container = new List<string>{
				"one", "two", "three"
			},
			singleValue = "exit",
			complex = new { childThatExists = "yes!" , childThatIsNull = aNullThing}
		};

		[Test]
		public void can_use__Or__predicates()
		{
			var result = Check.That(subject).singleValue[Should.Equal("enter").Or(Should.Equal("exit"))];

			Assert.That(result.Success, Is.True, result.Reason);
		}

		[Test]
		public void can_use__Or__predicates_failure_case()
		{
			var result = Check.That(subject).singleValue[Should.Equal("wrong").Or(Should.Equal("wronger"))];

			Assert.That(result.Success, Is.False);
			Assert.That(result.Reasons, Contains.Item("Outer.singleValue is not equal to wrong, is not equal to wronger"));
		}

		[Test]
		public void can_check_for_members()
		{
			var pass = Check.That(subject).complex[Should.HaveMember("childThatExists")];
			var fail = Check.That(subject).complex[Should.HaveMember("nonExistentChild")];

			Assert.That(pass.Success, Is.True, pass.Reason);
			Assert.That(fail.Success, Is.False);
		}

		[Test]
		public void can_check_for_members_null_case()
		{
			var fail = Check.That(subject).complex[Should.HaveMember("childThatIsNull")];

			Assert.That(fail.Success, Is.False);
			Assert.That(fail.Reasons, Contains.Item("Outer.complex did not contain member \"childThatIsNull\""));
		}

		[Test]
		public void can_check_for_members_negative_case()
		{
			var result = Check.That(subject).complex[Should.NotHaveMember("childThatIsNull")];

			Assert.That(result.Success, Is.True, result.Reason);
		}
		[Test]
		public void can_check_for_members_negative_case_failure()
		{
			var result = Check.That(subject).complex[Should.NotHaveMember("childThatExists")];

			Assert.That(result.Success, Is.False);
			Assert.That(result.Reasons, Contains.Item("Outer.complex contained unexpected member \"childThatExists\""));
		}

		[Test]
		public void exact_item_count()
		{
			var pass = Check.That(subject).container[Should.Have(3)];
			var fail = Check.That(subject).container[Should.Have(42)];

			Assert.That(pass.Success);
			Assert.That(fail.Success, Is.False);
			Assert.That(fail.Reasons, Contains.Item("Outer.container should have 42 items exactly, but had 3"));
		}
		
		[Test]
		public void __should_be__checks_target_against_a_lambda()
		{
			// passing a lambda in a closure works around the dynamic issues.
			var pass = Check.That(subject).container[Should.Be(o=>o != null, "is null, and that's bad!")];
			var fail = Check.That(subject).container[Should.Be(o=>o == null, "is not null... unexpected!")];

			Assert.That(pass.Success);
			Assert.That(fail.Success, Is.False);
			Assert.That(fail.Reasons, Contains.Item("Outer.container is not null... unexpected!"));
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
		public void can_check_no_items_match()
		{
			var pass = Check.That(subject).container[Should.HaveNone(o=>o as string=="four", "four found!")];
			var fail = Check.That(subject).container[Should.HaveNone(o=>o as string=="three", "three found!")];

			Assert.That(pass.Success, Is.True, pass.Reason);
			Assert.That(fail.Success, Is.False);
			Assert.That(fail.Reasons, Contains.Item("Outer.container three found!"));
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
		public string singleValue;
		public object complex;
	}
}
