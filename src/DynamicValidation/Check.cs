using System;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using DynamicValidation.Reflection;
using DynamicValidation.SpecialPredicates;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace DynamicValidation {

	/// <summary>
	/// Experimental version of CHECK.
	/// </summary>
	public class Check : DynamicObject {

		readonly object subject;
		readonly List<ChainStep> chain;

		public static dynamic That (object subject) {
			return new Check(subject);
		}

		#region Building Chain
		Check (object subject) {
			this.subject = subject;
			chain = new List<ChainStep>();
		}

		Check (object subject, IEnumerable<ChainStep> oldChain, ChainStep nextItem) {
			this.subject = subject;
			chain = new List<ChainStep>(oldChain) { nextItem };
		}

		dynamic Add (ChainStep name) {
			return new Check(subject, chain, name);
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
			
			var predicates = AllConstraintsAsPredicates(indexes);

			// First, check that the path is valid given the object tree.
			// try to ignore enumeration stuff as much as possible.
			AssertPath(result);
			if ( ! result.Success) return true;

			// Next check the validation rules against the object tree.
			// this is where the [single, any ...] rules come in.
			RunPredicates(result, predicates);

			return true;
		}

		void RunPredicates(Result outp, IEnumerable<INamedPredicate> predicates)
		{
			// TODO: build up a tree-walking strategy here
			// should be able to step through simple chain-steps
			// and recurse through enumerable ones.
			foreach (var predicate in predicates)
			{
				string message;
				if ( ! predicate.Matches(outp.Target, out message)) outp.FailBecause(message);
			}
		}


		IEnumerable<INamedPredicate> AllConstraintsAsPredicates(IEnumerable<object> indexes)
		{
			var constraintsAsPredicates = new List<INamedPredicate>();
			foreach (var index in indexes)
			{
				if (index is INamedPredicate)
				{
					constraintsAsPredicates.Add((INamedPredicate)index);
				} else if (index is IResolveConstraint)
				{
					constraintsAsPredicates.Add(new ConstraintPredicate((IResolveConstraint)index));
				} else
				{
					throw new ArgumentException("All assertions must be IResolveContraint or INamedPredicate");
				}
			}
			return constraintsAsPredicates;
		}
		void AssertPath(Result result)
		{
			// here we will walk through the 'chain' checking that members are defined
			// (i.e. the member exists and the user has spelled it correctly)
			// and are not null. If the LAST item in the chain is null, that's OK.
			// we fall over here if any enumerables are empty, because it screws with
			// type reflection.
			object currentTarget = subject;
			string pathSoFar = subject.GetType().Name;

			try
			{
				foreach (var step in chain)
				{
					currentTarget = AssertPathStep(result, step, currentTarget, ref pathSoFar);
				}
			} catch (FastFailureException) { }
		}

		static object AssertPathStep(Result result, ChainStep step, object currentTarget, ref string pathSoFar)
		{
			if (currentTarget == null)
			{
				result.FailBecause(pathSoFar + " is null or can't be accessed");
				throw new FastFailureException();
			}

			currentTarget = SkipOverEnumerable(currentTarget);
			if (currentTarget == null)
			{
				result.FailBecause(pathSoFar + " has no items");
				throw new FastFailureException();
			}

			var definitionCount = currentTarget.CountDefinitions(step.Name);

			if (definitionCount < 1)
			{
				if (currentTarget is IEnumerable<object>)
				{
					result.FailBecause(pathSoFar + " does not have a single child");
				}
				else
					result.FailBecause(pathSoFar + "." + step.Name + " is not a possible path");
				throw new FastFailureException();
			}
			if (definitionCount > 1)
			{
				result.FailBecause(pathSoFar + "." + step.Name + " is ambiguous");
				throw new FastFailureException();
			}

			pathSoFar += "." + step.Name;
			currentTarget = currentTarget.Get(step.Name);
			return currentTarget;
		}

		static object SkipOverEnumerable(object currentTarget)
		{
			if ( ! (currentTarget is IEnumerable<object>)) return currentTarget;
			return ((IEnumerable<object>) currentTarget).FirstOrDefault();
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
			
			/// <summary> Single string reason for failure </summary>
			public string Reason
			{
				get { return string.Join(", ", Reasons);}
			}

			/// <summary> Final object tested. If null is encountered before tested object, this will be null </summary>
			public object Target { get; set; }
		}
	}
}