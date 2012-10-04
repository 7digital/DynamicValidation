using System.Collections.Generic;
using NUnit.Framework;

namespace DynamicValidation.Tests
{
	[TestFixture]
	public class grouping_validations_into_one_result
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
		public void can_run_several_validators_together_and_get_merged_results()
		{
			var result = Check.With(subject,
				s => s.container(0).a[Should.BeTrue],
				s => s.container(1).a[Should.BeFalse],
				s => s.container(2).a[Should.BeTrue]
			);

			Assert.That(result.Success, Is.True, result.Reason);
		}

		[Test]
		public void can_run_several_validators_together_and_get_merged_results_with_failure()
		{
			var result = Check.With(subject,
				s => s.container(0).a[Should.BeTrue],
				s => s.container(1).a[Should.BeTrue],
				s => s.container(2).a[Should.BeTrue]
			);

			Assert.That(result.Success, Is.False);
			Assert.That(result.Reason, Is.EqualTo("X.container[1].a expected True but got False"));
		}
	}
}
