using System;
using System.Collections.Generic;
using DynamicValidation.SpecialPredicates;
using NUnit.Framework;
// ReSharper disable PossibleNullReferenceException
#pragma warning disable 168

namespace DynamicValidation.Tests {
	[TestFixture]
	public class simple_object_tree_tests {
		BaseThing subject;

		[SetUp]
		public void Setup () {
			subject = new BaseThing {
				One = new A(),
				Two = new B {
					X = new A{Value="Hello, world"},
					Y = null,
					Z = new B()
				}
			};
		}

		[Test]
		public void can_NOT_use_method_groups_in_index ()
		{
			// This doesn't work:
			//     Check.That(subject).One[MethodGroup];
			// it's just a restriction of C#
			// can work around it with "Should.Be"
			var x = Check.That(subject).One[Should.Be(MethodGroup, "ok")];
			Assert.Pass();
		}

		[Test]
		public void can_check_against_a_list_of_acceptable_values()
		{
			var ok = new List<string>{"Hello, John", "Hello, world", "Hello, universe"};
			var result = Check.That(subject).Two.X.Value[Should.EqualOneOf(ok)];

			Assert.That(result.Success, Is.True, result.Reason);
		}
		[Test]
		public void can_check_against_a_list_of_acceptable_values_failure_case()
		{
			var ok = new List<string>{"Hello, John", "Hello, Simon", "Hello, Jeff"};
			var result = Check.That(subject).Two.X.Value[Should.EqualOneOf(ok)];

			Assert.That(result.Success, Is.False);
			Assert.That(result.Reasons, Contains.Item("BaseThing.Two.X.Value got \"Hello, world\" which is not an acceptable value"));
		}

		[Test]
		public void can_get_given_predicate_messages () {
			var result = Check.That(subject).One[Should.BeNull];
			Assert.That(result.Success, Is.False);
			Assert.That(result.Reasons, Contains.Item("BaseThing.One was not null"));
		}

		[Test]
		public void assert_two_y_is_null () {
			Check.Result result = Check.That(subject).Two.Y[Should.BeNull];
			Assert.That(result.Success, Is.True);
		}

		[Test]
		public void assert_two_x_is_type_of_a () {
			Check.Result result = Check.That(subject).Two.X[Should.Be<A>()];
			Assert.That(result.Success, Is.True);
		}

		[Test]
		public void can_use_more_than_one_assertion () {
			Check.Result result = Check.That(subject).Two.X[Should.Be<A>(), Should.Equal(subject.Two.X)];
			Assert.That(result.Success, Is.True);
		}

		[Test]
		public void fails_if_any_assertion_fails () {
			Check.Result result = Check.That(subject).Two.X[Should.Be<A>(), Should.Equal(subject.Two.X), Should.BeNull];

			Console.WriteLine(string.Join(" ",result.Reasons));

			Assert.That(result.Success, Is.False);
		}

		[Test]
		public void can_mix_predicates_and_assertions () {
			var SaysHelloWorld = new NamedPredicate(o => (o as A).Value == "Hello, world", "Should greet the world");

			Check.Result result = Check.That(subject).Two.X[Should.Be<A>(), SaysHelloWorld];

			Assert.That(result.Success, Is.True);
		}

		[Test]
		public void can_mix_predicates_and_assertions_with_failure () {
			var FrankieSaysRelax = new NamedPredicate(o=> (o as A).Value == "Relax", "You should relax more");

			Check.Result result = Check.That(subject).Two.X[Should.Be<A>(), FrankieSaysRelax];

			Assert.That(result.Success, Is.False);
			Assert.That(result.Reasons, Contains.Item("BaseThing.Two.X You should relax more"));
		}

		[Test]
		public void assert_two_q_doesnt_exist () {
			Check.Result result = Check.That(subject).Two.Q.AnotherName.What.The.Fudge[Should.NotBeNull];
			Assert.That(result.Success, Is.False);
			Assert.That(result.Reasons, Contains.Item("BaseThing.Two.Q is not a valid path"));
		}

		#region Junk

		public bool MethodGroup (object thing) {
			return true;
		}

		#endregion
	}

	#region type junk
	public class B {
		public object Y { get; set; }
		public A X { get; set; }
		public object Z { get; set; }
	}

	public class A {
		public string Value { get; set; }
	}

	class BaseThing {
		public B Two { get; set; }
		public A One { get; set; }

	}
	#endregion
}
