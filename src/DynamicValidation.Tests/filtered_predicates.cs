using NUnit.Framework;

namespace DynamicValidation.Tests
{
	[TestFixture]
	public class filtered_predicates
	{
		readonly object subject = new { 
			a = new []{
				new {
					x = "yes",
					y = 1
				},
				new {
					x = "yes",
					y = 2
				},
				new {
					x = "no",
					y = 3
				},
				new {
					x = "maybe",
					y = 4
				}
			}
		};
		
		
		[Test]
		public void should_be_able_to_filter_selected_nodes_before_assertion()
		{
			Check.Result r = 
				Check.That(subject).a[Should.Have(2).Where(v=>v.x == "yes")];

			Assert.That(r.Success, Is.True, r.Reason);
		}
		
		[Test]
		public void should_be_able_to_filter_selected_nodes_before_assertion__failure_case()
		{
			Check.Result r = 
				Check.That(subject).a[Should.Have(2).Where(v=>v.x == "no")];

			Assert.That(r.Success, Is.False);
			Assert.That(r.Reason, Is.StringContaining(".a should have 2 items exactly, but had 1"));
		}
	}
}