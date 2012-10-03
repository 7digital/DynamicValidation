using System;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using DynamicValidation.Reflection;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace DynamicValidation {

	/// <summary>
	/// Experimental version of CHECK.
	/// </summary>
	public class Check2 : DynamicObject {

		readonly object subject;
		readonly List<ChainStep> chain;

		public static dynamic That (object subject) {
			return new Check2(subject);
		}

		#region Building Chain
		Check2 (object subject) {
			this.subject = subject;
			chain = new List<ChainStep>();
		}

		Check2 (object subject, IEnumerable<ChainStep> oldChain, ChainStep nextItem) {
			this.subject = subject;
			chain = new List<ChainStep>(oldChain) { nextItem };
		}

		dynamic Add (ChainStep name) {
			return new Check2(subject, chain, name);
		}

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			// we've been passed an enumeration argument
			switch (args.Length)
			{
				case 1: // either like `list(1)` or `list("any")`
					if (args[0] is int) result = Add(ChainStep.Complex(binder.Name, "single", (int)args[0], ChainStep.Anything));
					else result = Add(ChainStep.Complex(binder.Name, args[0].ToString(), 0, ChainStep.Anything));
					break;
				case 2: // like `list("all", o => o.something != null)`
					result = Add(ChainStep.Complex(binder.Name, args[0].ToString(), 0, (Func<object, bool>)args[1]));
					break;
				default: // don't understand!
					throw new ArgumentException("I don't understand the arguments to "+binder.Name);
			}
			return true;
		}

		public override bool TryGetMember (GetMemberBinder binder, out object result) {
			// No actual access here, just record the request.
			result = Add(ChainStep.SimpleStep(binder.Name));
			return true;
		}
		#endregion

		public override bool TryGetIndex (GetIndexBinder binder, object[] indexes, out object finalResult) {
			var result = new Result();
			finalResult = result;
			
			if (indexes.Any(i=> (i is IResolveConstraint || i is INamedPredicate) == false))
				throw new ArgumentException("All assertions must be IResolveContraint or INamedPredicate");


			AssertPathAndAssignResultTarget(result);

			if ( ! result.Success) return true;

			RunAssertions(result, indexes.OfType<IResolveConstraint>());
			RunPredicates(result, indexes.OfType<INamedPredicate>());

			return true;
		}

		void RunPredicates(Result outp, IEnumerable<INamedPredicate> predicates)
		{
			foreach (var predicate in predicates)
			{
				string message;
				if ( ! predicate.Matches(outp.Target, out message)) outp.FailBecause(message);
			}
		}

		void RunAssertions(Result outp, IEnumerable<IResolveConstraint> constraints)
		{
			foreach (var constraint in constraints)
			{
				var check = constraint.Resolve();
				if ( ! check.Matches(outp.Target))
				{
					var sw = new TextMessageWriter();
					check.WriteMessageTo(sw);
					outp.FailBecause(sw.ToString());
				}
			}
		}

		void AssertPathAndAssignResultTarget(Result result)
		{
			// here we will walk through the 'chain' checking that members are defined
			// (i.e. the member exists and the user has spelled it correctly)
			// and are not null. If the LAST item in the chain is null, that's OK.
			object currentTarget = subject;
			string pathSoFar = subject.GetType().Name;

			foreach (var step in chain)
			{
				if (currentTarget == null)
				{
					result.FailBecause(pathSoFar + " is null or can't be accessed");
					return;
				}

				currentTarget = ShortcutTargetIfSingleItemEnumerable(currentTarget);

				var definitionCount  = currentTarget.CountDefinitions(step.Name);

				if (definitionCount < 1)
				{
					if (currentTarget is IEnumerable<object>)
					{
						result.FailBecause(pathSoFar + "." + step.Name + " does not have a single child");
					}
					else
						result.FailBecause(pathSoFar + "." + step.Name + " is not a possible path");
					return;
				}
				if (definitionCount > 1)
				{
					result.FailBecause(pathSoFar + "." + step.Name + " is ambiguous");
					return;
				}

				pathSoFar += "."+step.Name;
				currentTarget = currentTarget.Get(step.Name);
			}

			result.Target = currentTarget;
		}

		static object ShortcutTargetIfSingleItemEnumerable(object currentTarget)
		{
			if ( ! (currentTarget is IEnumerable<object>)) return currentTarget;

			try
			{
				return ((IEnumerable<object>) currentTarget).Single();
			} catch
			{
				return currentTarget;
			}
		}
		
		class ChainStep
		{
			public string Name { get; set; }
			public int SingleIndex { get; set; }
			public ListAssertion ListAssertionType { get; set; }
			public Func<object, bool> FilterPredicate { get; set; }

			public static ChainStep SimpleStep(string name)
			{
				return new ChainStep{
					Name = name,
					ListAssertionType = ListAssertion.Single,
					FilterPredicate = Anything,
					SingleIndex = 0
				};
			}

			public static ChainStep Complex(string name, string assertionName, int index, Func<object, bool> filter)
			{
				return new ChainStep{
					Name = name,
					ListAssertionType = Guess(assertionName),
					FilterPredicate = filter,
					SingleIndex = index
				};
			}

			static ListAssertion Guess(string s)
			{
				switch (s.ToLowerInvariant())
				{
					case "any":return ListAssertion.Any;
					case "all":return ListAssertion.All;
					case "single":return ListAssertion.Single;
					default: return ListAssertion.Single;
				}
			}

			public static bool Anything(object arg) { return true; }
		}

		public class Result {
			public Result()
			{
				Success = true;
				Reasons = new List<string>();
			}

			internal void FailBecause(string reason)
			{
				Success = false;
				((List<string>)Reasons).Add(reason);
			}

			/// <summary> True if all predicates passed. False if any failed  </summary>
			public bool Success { get; set; }

			/// <summary> Description of failures if not success. Undefined otherwise. </summary>
			public IEnumerable<string> Reasons { get; set; }

			/// <summary> Final object tested. If null is encountered before tested object, this will be null </summary>
			public object Target { get; set; }
		}
	}

	enum ListAssertion
	{
		Single = 0, // default!
		All = 1, 
		Any = 2
	}
}