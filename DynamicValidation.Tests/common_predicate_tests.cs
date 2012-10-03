using System.Collections.Generic;
using NUnit.Framework;

namespace DynamicValidation.Tests
{
	[TestFixture]
	public class common_predicate_tests
	{
		readonly object subject = new {container = new List<string>{
			"one", "two", "three"
		}};

		[Test]
		public void exact_item_count()
		{
			var pass = Check.That(subject).container[Should.Have(3)];
			var fail = Check.That(subject).container[Should.Have(42)];

			Assert.That(pass.Success);
			Assert.That(fail.Success, Is.False);
			Assert.That(fail.Reasons, Contains.Item("Should have 42 items exactly"));
		}
		
		[Test]
		public void minimum_item_count()
		{
			var pass = Check.That(subject).container[Should.HaveAtLeast(2)];
			var fail = Check.That(subject).container[Should.HaveAtLeast(42)];

			Assert.That(pass.Success);
			Assert.That(fail.Success, Is.False);
			Assert.That(fail.Reasons, Contains.Item("Should have at least 42 items"));
		}
		
		[Test]
		public void maximum_item_count()
		{
			var pass = Check.That(subject).container[Should.HaveLessThan(4)];
			var fail = Check.That(subject).container[Should.HaveLessThan(3)];

			Assert.That(pass.Success);
			Assert.That(fail.Success, Is.False);
			Assert.That(fail.Reasons, Contains.Item("Should have less than 3 items"));
		}
	}
}
