using System;
using System.Collections.Generic;
using NUnit.Framework;
// ReSharper disable PossibleNullReferenceException
#pragma warning disable 168

namespace DynamicValidation.Tests
{
	[TestFixture]
	public class enumerables_in_tree_tests
	{
		ComplexThing subject;

		[SetUp]
		public void Setup()
		{
			subject = new ComplexThing
			{
				SingleThing = new J
				{
					ListOfK = new List<K> {
						new K{Value = "Hello, Bob"},
						new K{Value = "Hello, Eve"},
						new K{Value = "Hello, Alice"}
					}
				},
				JustOneThingInAList = new List<K> {
					new K{Value = "Woop!"}
				}
			};
		}

		[Test]
		public void can_assert_on_enumerables()
		{
			var r1 = Check.That(subject).SingleThing.ListOfK[Should.Have(3)];
			Assert.That(r1.Success, Is.True, r1.Reason);
		}

		/*
		[Test]
		public void can_assert_on_enumerables_and_their_results()
		{
			Check.Result r1 = Check.That(subject).SingleThing.ListOfK[Should.Have(3)];

			var r2 = Check.First(r1.Target).Value[Is.EqualTo("Hello, Bob")];

			Assert.That(r1.Success);
			Assert.That(r2.Success);
		}

		[Test]
		public void can_assert_on_single_enumerables()
		{
			var result = Check.Single(subject.JustOneThingInAList).Value[Is.EqualTo("Woop!")];
			Assert.That(result.Success);
		}

		[Test]
		public void throws_if_single_called_on_non_single_list()
		{
			Assert.Throws<InvalidOperationException>(() =>
			{
				var x = Check.Single(subject.SingleThing.ListOfK).Value[Is.EqualTo("Woop!")];
			});
		}

		[Test]
		public void can_shortcut_enumerables_when_it_contains_one_item()
		{
			var result = Check.Single(subject).JustOneThingInAList.Value[Is.EqualTo("Woop!")];
			Assert.That(result.Success, string.Join(" ", result.Reasons));
		}*/

		[Test]
		public void can_assert_that_nth_child_validates()
		{
			var result = Check.That(subject).SingleThing.ListOfK(2).Value[Is.EqualTo("Hello, Alice")];

			Assert.That(result.Success, Is.True, result.Reason);
		}

		[Test]
		public void can_assert_that_all_children_validate()
		{
			var result = Check.That(subject).SingleThing.ListOfK("all").Value[Is.StringContaining("Hello")];

			Assert.That(result.Success, Is.True, result.Reason);
		}

	}

	#region type junk
	class ComplexThing
	{
		public J SingleThing { get; set; }
		public IEnumerable<K> JustOneThingInAList { get; set; }
	}

	class J
	{
		public IEnumerable<K> ListOfK { get; set; }
	}

	class K
	{
		public string Value { get; set; }
	}
	#endregion
}
