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

		[Test]
		public void can_assert_that_nth_child_validates()
		{
			var result = Check.That(subject).SingleThing.ListOfK(2).Value[Should.Equal("Hello, Alice")];

			Assert.That(result.Success, Is.True, result.Reason);
		}

		[Test]
		public void can_assert_that_all_children_validate()
		{
			var result = Check.That(subject).SingleThing.ListOfK("all").Value[Should.Contain("Hello")];

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
