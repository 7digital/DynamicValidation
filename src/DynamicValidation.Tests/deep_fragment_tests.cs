using NUnit.Framework;

namespace DynamicValidation.Tests
{
	[TestFixture]
	public class deep_fragment_tests
	{
		readonly object subject = new { 
			a = new { deep = new { way = new { down = new {
				further = new {Value="Hi"},
				different = new {Path = "There"}
			}}}}};

		[Test]
		public void should_be_able_to_assert_multiple_routes_from_one_stem()
		{
			var stem = Check.That(subject).a.deep.way.down;

			var r1 = stem.further[Should.NotBeNull];
			var r2 = stem.different.Path[Should.NotBeEmpty];
			var r3 = stem.different.Path[Should.Equal("Howdy")];

			Assert.That(r1.Success, Is.True, r1.Reason);
			Assert.That(r2.Success, Is.True, r2.Reason);

			Assert.That(r3.Success, Is.False);
			Assert.That(r3.Reason, Is.StringContaining(".a.deep.way.down.different.Path is not equal to Howdy"));
		}

		[Test]
		public void should_be_able_to_group_assert_on_a_stem()
		{
			var stem = Check.That(subject).a.deep.way.down;

			var pass = Check.With((Check)stem, 
				s=>s.further[Should.NotBeNull],
				s=>s.different.Path[Should.NotBeEmpty]
			);
			var fail = Check.With((Check)stem, 
				s=>s.different.Path[Should.Equal("Howdy")]
			);

			
			Assert.That(pass.Success, Is.True, pass.Reason);

			Assert.That(fail.Success, Is.False);
			Assert.That(fail.Reason, Is.StringContaining(".a.deep.way.down.different.Path is not equal to Howdy"));
		}
	}
}
