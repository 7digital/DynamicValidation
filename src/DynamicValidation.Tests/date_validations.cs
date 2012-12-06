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
		public void Should_be_able_to_check_dates_in_one_path_against_dates_in_another_path_failing()
		{
			var obj = new
			{
				earlier = new {
					date = new DateTime(1980, 01, 01)
				},
				later = new {
					date = new DateTime(1979, 01, 01)
				}
			};

			Check.Result result = Check.That(obj).earlier.date[Should.BeBefore(Check.That(obj).later.date)];

			Console.WriteLine(result.Reason);

			Assert.That(result.Success, Is.False, result.Reason);
			Assert.That(result.Reason, Is.StringContaining(".earlier.date should be before"));
			Assert.That(result.Reason, Is.StringEnding(".later.date"));
		}

		[Test]
		public void Should_be_able_to_check_dates_in_one_path_against_dates_in_another_path_passing()
		{
			var obj = new
			{
				earlier = new {
					date = new DateTime(1979, 01, 01)
				},
				later = new {
					date = new DateTime(1980, 01, 01)
				}
			};

			Check.Result result = Check.That(obj).earlier.date[Should.BeBefore(Check.That(obj).later.date)];

			Console.WriteLine(result.Reason);

			Assert.That(result.Success, Is.True, result.Reason);
		}

		[Test]
		public void Should_pass_validation_when_date_is_null()
		{
			var obj = new
			{
				earlier = new K
				{
					Date = new DateTime(1979, 01, 01)
				},
				later = new K
				{
					Date = null
				}
			};

			Check.Result result = Check.That(obj).earlier.Date[Should.BeNullOrBefore(Check.That(obj).later.Date)];

			Console.WriteLine(result.Reason);

			Assert.That(result.Success, Is.True, result.Reason);
		}
		class K
		{
			public DateTime? Date { get; set; }
		}
	}
	
}