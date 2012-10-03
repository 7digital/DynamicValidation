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
				}
			};
		}

		[Test]
		public void can_assert_that_all_children_validate()
		{
			var result = Check.That(subject).SingleThing.ListOfK().Value[Is.StringContaining("Hello")];
			Assert.That(result.Success, Is.True, string.Join(" ", result.Reasons));
		}
	}

	#region type junk
	class ComplexThing
	{
		public J SingleThing { get; set; }

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
