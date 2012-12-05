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
					new DateTime(1980, 01, 01),
				}
			};
			Check.Result result = Check.That(obj).dates("all")[Should.BeBefore(new DateTime(1012, 01, 01))];

			Assert.That(result.Success, Is.False, result.Reason);
		}
	}
}