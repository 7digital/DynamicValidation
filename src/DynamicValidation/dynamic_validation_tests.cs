using System;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DynamicValidation {
	[TestFixture]
	public class dynamic_validation_tests {
		BaseThing subject;

		[SetUp]
		public void Setup () {
			subject = new BaseThing {
				One = new A(),
				Two = new B {
					X = new A(),
					Y = null,
					Z = new B()
				}
			};
		}


		[Test]
		public void quick_test () {
			var result = Check.That(subject).can.get.just.about.anything.from.a_dynamic["and", "like", "it"];
			Assert.That(result, Is.EqualTo("can get just about anything from a_dynamic and like it"));
		}

		[Test]
		public void cant_use_method_groups_in_index () {
			//This doesn't work: // var result = Check.That(subject).something[MethodGroup];
			var result = Check.That(subject).something[Goes.LikeThis]; // but this is ok.
		}

		[Test]
		public void can_get_given_predicate_names () {
			var result = Check.That(subject).something[hasValue: Is.Not.Null];
			Console.WriteLine(result);
		}

		[Test, Ignore("Not yet implemented")]
		public void assert_two_y_is_null () {
			Check.Result result = Check.That(subject).Two.Y[Is.Null];
			Assert.That(result.Success, Is.True);
		}

		[Test, Ignore("Not yet implemented")]
		public void assert_two_x_is_type_of_a () {
			Check.Result result = Check.That(subject).Two.X[Is.InstanceOf<A>()];
			Assert.That(result.Success, Is.True);
		}

		[Test, Ignore("Not yet implemented")]
		public void assert_two_q_doesnt_exist () {
			Check.Result result = Check.That(subject).Two.Q[Is.Not.Null];
			Assert.That(result.Success, Is.False);
			Assert.That(result.Reasons, Contains.Item("BaseThing.Two.Q is not a possible path"));
		}

		/*
		 * The idea is to get the check object, walk the object tree
		 * then pass a set of validation predicates into the index.
		 * 
		 * Then get a result back, success or failure, with a message
		 * for failure which is the name of the predicates that failed.
		 * 
		 * If any part of the object tree is null except the last item
		 * then that's a failure with it's own message.
		 * 
		 * Not entirely sure how to handle lists/enumerations. Probably
		 * something like
		 * 
		 *     Check.That(thing).a.b(Has.All)[...]
		 *     
		 * If the validation needs to check both aspect of an enumerable
		 * and it's contents, should split the tests up.
		 * 
		 *     Check.That(thing).a.b[Has.AtLeastOne]
		 *     Check.That(thing).a.b(All).c[Is.NotEmpty]
		 *     
		 * or something like that.
		 */
		#region Junk
		
		class Goes {
			public static Func<object, bool> LikeThis { get { return o => true; } }
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

	public class A { }

	class BaseThing {
		public B Two { get; set; }
		public A One { get; set; }

	}
	#endregion
}
