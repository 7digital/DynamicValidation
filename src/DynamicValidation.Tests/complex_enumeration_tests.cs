using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DynamicValidation.Tests
{
	[TestFixture]
	public class complex_enumeration_tests
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
			emptyItem = new List<object>(),
			listOfStrings = new List<string> {
				"a_", "b_", "c_", "d_", "e_", "f_"
			}
		};

		[Test]
		[TestCase(0, true)]
		[TestCase(1, false)]
		[TestCase(2, true)]
		public void can_check_that_nth_item_validates(int n, bool a_value)
		{
			var result = Check.That(subject).container(n).a[Should.Equal(a_value)];

			Assert.That(result.Success, Is.True, string.Join(" ", result.Reasons));
		}

		[Test]
		public void all_child_validation_fails_correctly()
		{
			var result = Check.That(subject).container("all").a[Should.BeTrue];

			Assert.That(result.Success, Is.False, "check did not fail, but it should have");
			Assert.That(result.Reasons, Contains.Item("X.container[1].a expected True but got False"));
		}

		[Test]
		public void all_child_validation_passes_correctly()
		{
			var result = Check.That(subject).container("all").b[Should.BeTrue];

			Assert.That(result.Success, Is.True, result.Reason);
		}

		[Test]
		public void any_child_validation_passes_correctly()
		{
			var result = Check.That(subject).container("any").a[Should.BeTrue];

			Assert.That(result.Success, Is.True);
		}

		[Test]
		public void any_child_validation_fails_correctly()
		{
			var result = Check.That(subject).container("any").b[Should.BeFalse];

			Assert.That(result.Success, Is.False);
			Assert.That(result.Reasons, Contains.Item("X.container.b expected False but got True"));
		}

		[Test]
		public void single_child_validation_fails_on_multiple_items()
		{
			var result = Check.That(subject).container("single").a[Should.BeFalse];

			Assert.That(result.Success, Is.False);
			Assert.That(result.Reasons, Contains.Item("X.container has length of 3, expected 1"));
		}

		[Test]
		public void single_child_validation_fails_on_no_items()
		{
			var result = Check.That(subject).emptyItem("single").a[Should.BeFalse];

			Assert.That(result.Success, Is.False);
			Assert.That(result.Reasons, Contains.Item("X.emptyItem has no items"));
		}

		[Test]
		public void single_child_validation_passes_with_one_item()
		{
			var result = Check.That(subject).singleItem("single").a[Should.BeTrue];

			Assert.That(result.Success, Is.True, result.Reason);
		}

		[Test]
		public void can_use_predicates_to_filter_deep_enumerations()
		{
			var message = new FakeMessage();
 
			var IsBundle = Should.Be(o => ((Release) o).ReleaseTypes.Single().Value == "Bundle", "Bundle");
			var IsTrack = Should.Be(o => ((Release) o).ReleaseTypes.Single().Value == "TrackRelease", "Track release");

			// This checks that we have exactly 1 bundle release that has a non-empty ICPN
			// and that all track releases have non-empty ISRCs
			// (note that 'ReleaseIds' has an implicit "single" specification)
			var result1 = Check.That(message).Releases("single", IsBundle).ReleaseIds.ICPN.Value[Should.NotBeEmpty];
			var result2 = Check.That(message).Releases("all", IsTrack).ReleaseIds.ISRC.Value[Should.NotBeEmpty];

			Assert.That(result1.Success, Is.True, "Bundle ICPN: " + result1.Reason);
			Assert.That(result2.Success, Is.True, "Track ISRC: " + result2.Reason);
		}

		[Test]
		public void can_use_predicates_to_filter_deep_enumerations_failure_case_single()
		{
			var message = new FakeMessage();
 
			var IsBundle = Should.Be(o => ((Release) o).ReleaseTypes.Single().Value == "Bundle", "Bundle");

			var result = Check.That(message).Releases("single", IsBundle).ReleaseIds.ICPN.Value[Should.BeEmpty];

			Assert.That(result.Success, Is.False);
			Assert.That(result.Reasons, Contains.Item("FakeMessage.Releases(Matching: Bundle).ReleaseIds.ICPN.Value should be empty but is \"bundle icpn\""));
		}
		[Test]
		public void can_use_predicates_to_filter_deep_enumerations_failure_case_all()
		{
			var message = new FakeMessage();
 
			var IsTrack = Should.Be(o => ((Release) o).ReleaseTypes.Single().Value == "TrackRelease", "Track release");

			var result = Check.That(message).Releases("all", IsTrack).ReleaseIds.ISRC.Value[Should.BeEmpty];

			Assert.That(result.Success, Is.False);
			Assert.That(result.Reasons, Contains.Item("FakeMessage.Releases(Matching: Track release)[0].ReleaseIds.ISRC.Value should be empty but is \"first track isrc\""));
			Assert.That(result.Reasons, Contains.Item("FakeMessage.Releases(Matching: Track release)[1].ReleaseIds.ISRC.Value should be empty but is \"second track isrc\""));
		}

		[Test]
		public void can_validate_terminal_enumerable()
		{
			var result = Check.That(subject).listOfStrings[Should.Have(6)];

			Assert.That(result.Success, Is.True, result.Reason);
		}
		
		[Test]
		public void can_validate_contents_of_terminal_enumerable()
		{
			var result = Check.That(subject).listOfStrings("all")[Should.Contain("_")];

			Assert.That(result.Success, Is.True, result.Reason);
		}
		[Test]
		public void can_validate_contents_of_terminal_enumerable_index()
		{
			var result = Check.That(subject).listOfStrings(1)[Should.Contain("_")];

			Assert.That(result.Success, Is.True, result.Reason);
		}
	}

	#region Type junk
	public class FakeMessage
	{
		public IList<Release> Releases { get; set; }

		public FakeMessage()
		{
			Releases = new List<Release> {
				new Release{
					ReleaseTypes = new List<ReleaseType>{new ReleaseType{Value = "Bundle"}},
					ReleaseIds = new List<ReleaseId> {
						new ReleaseId{ICPN = new ValueThing{Value="bundle icpn"}}
					}
				},
				new Release{
					ReleaseTypes = new List<ReleaseType>{new ReleaseType{Value = "TrackRelease"}},
					ReleaseIds = new List<ReleaseId> {
						new ReleaseId{ISRC = new ValueThing{Value="first track isrc"}}
					}
				},
				new Release{
					ReleaseTypes = new List<ReleaseType>{new ReleaseType{Value = "TrackRelease"}},
					ReleaseIds = new List<ReleaseId> {
						new ReleaseId{ISRC = new ValueThing{Value="second track isrc"}}
					}
				},
			};
		}
	}

	public class Release
	{
		public List<ReleaseType> ReleaseTypes { get; set; }
		public List<ReleaseId> ReleaseIds { get; set; }
	}

	public class ReleaseId
	{
		public ValueThing ICPN { get; set; }
		public ValueThing ISRC { get; set; }
	}

	public class ValueThing
	{
		public string Value { get; set; }
	}

	public class ReleaseType
	{
		public string Value { get; set; }
	}

	public class X
	{
		public List<object> container;
		public List<object> singleItem;
		public List<object> emptyItem;
		public List<string> listOfStrings;
	}
	#endregion
}
