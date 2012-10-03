using System;
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
			var x = Check.That(subject).One[ItShould.BeLikeThis];
			Assert.Pass();
		}

		[Test]
		public void can_get_given_predicate_names () {
			var result = Check.That(subject).One[Is.Not.Null];
			Assert.That(result.Success, Is.True);
		}

		[Test]
		public void terminal_object_is_returned_in_result () {
			Check.Result result = Check.That(subject).Two.X[Is.Not.Null];
			Assert.That(result.Target, Is.EqualTo(subject.Two.X));
		}

		[Test]
		public void assert_two_y_is_null () {
			Check.Result result = Check.That(subject).Two.Y[Is.Null];
			Assert.That(result.Success, Is.True);
		}

		[Test]
		public void assert_two_x_is_type_of_a () {
			Check.Result result = Check.That(subject).Two.X[Is.InstanceOf<A>()];
			Assert.That(result.Success, Is.True);
		}

		[Test]
		public void can_use_more_than_one_assertion () {
			Check.Result result = Check.That(subject).Two.X[Is.InstanceOf<A>(), Is.EqualTo(subject.Two.X)];
			Assert.That(result.Success, Is.True);
		}

		[Test]
		public void fails_if_any_assertion_fails () {
			Check.Result result = Check.That(subject).Two.X[Is.InstanceOf<A>(), Is.EqualTo(subject.Two.X), Is.Null];

			Console.WriteLine(string.Join(" ",result.Reasons));

			Assert.That(result.Success, Is.False);
		}

		[Test]
		public void can_mix_predicates_and_assertions () {
			var SaysHelloWorld = new NamedPredicate(o => (o as A).Value == "Hello, world", "Should greet the world");

			Check.Result result = Check.That(subject).Two.X[Is.InstanceOf<A>(), SaysHelloWorld];

			Assert.That(result.Success, Is.True);
		}

		[Test]
		public void can_mix_predicates_and_assertions_with_failure () {
			var FrankieSaysRelax = new NamedPredicate(o=> (o as A).Value == "Relax", "You should relax more");

			Check.Result result = Check.That(subject).Two.X[Is.InstanceOf<A>(), FrankieSaysRelax];

			Console.WriteLine(string.Join(" ", result.Reasons));

			Assert.That(result.Success, Is.False);
			Assert.That(result.Reasons, Contains.Item("You should relax more"));
		}

		[Test]
		public void assert_two_q_doesnt_exist () {
			Check.Result result = Check.That(subject).Two.Q.AnotherName.What.The.Fudge[Is.Not.Null];
			Assert.That(result.Success, Is.False);
			Assert.That(result.Reasons, Contains.Item("BaseThing.Two.Q is not a possible path"));
		}

		#region Junk
		
		public class ItShould {
			public static INamedPredicate BeLikeThis { get { return 
				new NamedPredicate(o => true, "test"); } }
		}

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
