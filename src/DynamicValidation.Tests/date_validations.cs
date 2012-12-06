using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DynamicValidation.Tests
{
	[TestFixture]
	public class date_validations
	{
		[Test]
		public void Should_validate_when_date_is_before_other_date()
		{
			var obj = new
			{
				dates = new []
				{
					new {Value = new DateTime(1980,01,01)}
				}
			};
			Check.Result result = Check.That(obj).dates("all").Value[Should.BeBefore(new DateTime(2012, 01, 01))];

			Assert.That(result.Success, Is.True, result.Reason);
		}

		[Test]
		public void Should_fail_when_date_is_after_other_date()
		{
			var obj = new
			{
				dates = new List<DateTime>
				{
					new DateTime(1980, 01, 01),
				}
			};
			var expectedDate = new DateTime(1012, 01, 01);
			Check.Result result = Check.That(obj).dates("all")[Should.BeBefore(expectedDate)];

			Assert.That(result.Success, Is.False, result.Reason);
			Assert.That(result.Reason, Is.StringEnding("should be before " + expectedDate));
		}


		[Test]
		public void Should_be_able_to_check_dates_in_one_path_against_dates_in_another_path()
		{
			var obj = new
			{
				earlier = new {
					date = new DateTime(1980, 01, 01)
				},
				later = new {
					date = new DateTime(1980, 01, 01)
				}
			};

			Check.Result result = Check.That(obj).earlier.date[Should.BeBefore(Check.That(obj).later.date)];

			Assert.That(result.Success, Is.False, result.Reason);
			Assert.That(result.Reason, Is.StringEnding(".earlier.date should be before 01/01/1980 00:00:00"));
		}
	}
}