using System;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using DynamicValidation.Reflection;
using DynamicValidation.SpecialPredicates;
using NUnit.Framework.Constraints;

namespace DynamicValidation {

	/// <summary>
	/// Experimental version of CHECK.
	/// </summary>
	public class Check : DynamicObject {
		public static dynamic That (object subject) {
			return new Check(subject);
		}

		#region Building Chain

		readonly object subject;
		readonly List<ChainStep> chain;

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
					if (args[0] is int) result = Add(ChainStep.Complex(binder.Name, "index", (int)args[0], ChainStep.Anything));
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

		#region Testing object tree
		public override bool TryGetIndex (GetIndexBinder binder, object[] indexes, out object finalResult) {
			var result = new Result();
			finalResult = result;
			
			var predicates = AllConstraintsAsPredicates(indexes);

			// Next check the validation rules against the object tree.
			// this is where the [single, any ...] rules come in.
			result.Target = subject;
			string pathSoFar = subject.GetType().Name;
			RunPredicates(chain, pathSoFar, result, predicates);

			return true;
		}

		static void RunPredicates(List<ChainStep> chain, string path, Result result, IEnumerable<INamedPredicate> predicates)
		{
			// Step through simple chain-steps
			// and recurse through enumerable ones.

			// each time we come to a [non-single / non nth] enumeration,
			// we split the chain and recursively check further.

			// when coming back up the tree, we merge in all the failure reasons.

			// we only actually run our predicates once we hit the end of the chain.
			var remainingChain = chain;
			while (remainingChain.Count > 1)
			{
				var step = remainingChain.First();
				remainingChain = remainingChain.Skip(1).ToList();

				var pathHere = path + "." + step.Name;
				object next;
				try {
					next = result.Target.Get(step.Name);
				} catch (FastFailureException) {
					result.FailBecause(pathHere + " is not a valid path");
					return;
				}

				if (next == null) {
					result.FailBecause(pathHere + " is null or not accessible");
					return;
				}

				if (next is IEnumerable<object>) {
					var container = ((IEnumerable<object>)next).Where(step.FilterPredicate).ToArray();
					switch (step.ListAssertionType) {
						case ListAssertion.Single:
							if (container.Length > 1) {
								result.FailBecause(pathHere + " has length of " + container.Length + ", expected 1");
								return;
							}
							if (container.Length < 1) {
								result.FailBecause(pathHere + " has no items");
								return;
							}
							next = container[0];
							break;

						case ListAssertion.Index:
							if (step.SingleIndex < 0 || step.SingleIndex >= container.Length) {
								result.FailBecause(pathHere + " has length of " + container.Length + ", tried to access index " + step.SingleIndex);
								return;
							}
							pathHere += "[" + step.SingleIndex + "]";
							next = container[step.SingleIndex];
							break;

						case ListAssertion.All:
							var i = 0;
							foreach (var route in container) {
								var cleanResult = new Result {Target = route};
								var localPath = pathHere + "[" + i + "]";
								RunPredicates(remainingChain, localPath, cleanResult, predicates);
								result.Merge(cleanResult);
								i++;
							}
							if ( ! result.Success) 
								result.FailBecause("not all children of " + pathHere + " validated successfully");
							return; // should have walked entire rest of tree.

						case ListAssertion.Any:
							var j = 0;
							var successCount = 0;
							var subResult = new Result();
							foreach (var route in container) {
								var cleanResult = new Result {Target = route};
								var localPath = pathHere + "[" + j + "]";
								RunPredicates(remainingChain, localPath, cleanResult, predicates);
								subResult.Merge(cleanResult);
								if (cleanResult.Success) successCount++;
								j++;
							}
							if (successCount < 1) {
								result.FailBecause("no children of " + pathHere + " validated successfully");
								result.Merge(subResult);
							}
							return; // should have walked entire rest of tree.

						default: throw new Exception("Unexpected list assertion type");
					}
				}

				result.Target = next;
				path = pathHere;
			}

			if (remainingChain.Count == 1) {
				var step = remainingChain[0];
				try {
					result.Target = result.Target.Get(step.Name);
					path += "." + step.Name;
				} catch (FastFailureException) {
					result.FailBecause(path + "." + step.Name + " is not a valid path");
					return;
				}
			}

			foreach (var predicate in predicates)
			{
				string message;
				if (!predicate.Matches(result.Target, out message)) result.FailBecause(path + " " + message);
			}
		}

		static IEnumerable<INamedPredicate> AllConstraintsAsPredicates(IEnumerable<object> indexes)
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
		#endregion

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

			internal object Target { get; set; }

			public void Merge(Result otherResult) {
				Success &= otherResult.Success;
				Reasons = Reasons.Union(otherResult.Reasons).ToList();
			}
		}
	}
}