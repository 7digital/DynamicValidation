using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DynamicValidation.Tests
{
	[TestFixture]
	public class enumerables_in_tree_tests
	{
		ComplexThing subject;

		[SetUp]
		public void Setup () {
			subject = new ComplexThing {
				SingleThing = new J {
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
			var HasThreeItems = new NamedPredicate(
				o => (o as IEnumerable<K>).Count() == 3,
				"Should have three items exactly"
				);

			var r1 = Check.That(subject).SingleThing.ListOfK[HasThreeItems];
			Assert.That(r1.Success);
		}

		
		[Test]
		public void can_assert_on_enumerables_and_their_results()
		{
			var hasThreeItems = new NamedPredicate(
				o => ((IEnumerable<K>)o).Count() == 3,
				"Should have three items exactly"
				);

			Check.Result r1 = Check.That(subject).SingleThing.ListOfK[hasThreeItems];

			var r2 = Check.First(r1.Target).Value[Is.EqualTo("Hello, Bob")];
			
			Assert.That(r1.Success);
			Assert.That(r2.Success);
		}

		[Test]
		public void can_assert_on_single_enumerables ()
		{
			var result = Check.Single(subject.JustOneThingInAList).Value[Is.EqualTo("Woop!")];
			Assert.That(result.Success);
		}

		[Test]
		public void throws_if_single_called_on_non_single_list ()
		{
			Assert.Throws<InvalidOperationException>(()=> {
				var x = Check.Single(subject.SingleThing.ListOfK).Value[Is.EqualTo("Woop!")];
			});
		}

		[Test]
		public void can_shortcut_enumerables_when_it_contains_one_item ()
		{
			var result = Check.Single(subject).JustOneThingInAList.Value[Is.EqualTo("Woop!")];
			Assert.That(result.Success, string.Join(" ", result.Reasons));
		}

		[Test]
		public void can_NOT_assert_that_all_children_validate()
		{
			// Plan is to be able to do something like this:
			//     Check.That(subject).SingleThing.ListOfK("all").Value[Is.StringContaining("Hello")];
			// but for now it just fails.
			var result = Check.That(subject).SingleThing.ListOfK.Value[Is.StringContaining("Hello")];

			Assert.That(result.Success, Is.False);
			Assert.That(result.Reasons, Contains.Item("ComplexThing.SingleThing.ListOfK.Value is inside an enumerable"));
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
