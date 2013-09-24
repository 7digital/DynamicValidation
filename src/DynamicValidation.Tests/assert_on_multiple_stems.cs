using NUnit.Framework;

namespace DynamicValidation.Tests
{
	[TestFixture]
	public class assert_on_multiple_stems
	{
		readonly object subject = new Root {
			one = new { 
				two = new {
					three = new {
						check = "Hello"
					}
				}
			},
			two = new {
				three = new {
					check = "Hello"
				}
			}
		};


		[Test]
		public void can_run_the_same_assertions_on_multiple_stems()
		{
			Check stem1 = Check.That(subject).one.two;
			Check stem2 = Check.That(subject).two;

			var result = Check.With(new []{stem1, stem2},
				two=>two.three.check[Should.Equal("Hello")]
				);
			
			Assert.That(result.Success, Is.True, result.Reason);
		}

		[Test]
		public void can_run_the_same_assertions_on_multiple_stems_failure_case()
		{
			var stem1 = Check.That(subject).one.two;
			var stem2 = Check.That(subject).two;

			var result = Check.With(new []{stem1, stem2},
				two=>two.three.check[Should.Equal("World")]
				);

			Assert.That(result.Success, Is.False);
			Assert.That(result.Reasons, Contains.Item("Root.one.two.three.check is not equal to World"));
			Assert.That(result.Reasons, Contains.Item("Root.two.three.check is not equal to World"));
		}

		[Test]
		public void can_run_the_same_assertions_on_multiple_stems_OR_case()
		{
			var stem1 = Check.That(subject).one;
			var stem2 = Check.That(subject).two;
			
			var result = Check.AtLeastOne(new []{stem1, stem2},
				two=>two.three.check[Should.Equal("Hello")]
				);

			Assert.That(result.Success, Is.True);
		}
		
		[Test]
		public void can_run_the_same_assertions_on_multiple_stems_OR_failure_case()
		{
			var stem1 = Check.That(subject).one;
			var stem2 = Check.That(subject).two;
			
			var result = Check.AtLeastOne(new []{stem1, stem2},
				two=>two.three.check[Should.Equal("World")]
				);

			Assert.That(result.Success, Is.False);
			Assert.That(result.Reasons, Contains.Item("Root.one.three is not a valid path"));
			Assert.That(result.Reasons, Contains.Item("Root.two.three.check is not equal to World"));
		}
	}

	class Root
	{
		public object one;
		public object two;
	}
}
