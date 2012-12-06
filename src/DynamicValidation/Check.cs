using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using DynamicValidation.Internals;
using DynamicValidation.Reflection;

namespace DynamicValidation {
	public class Check : DynamicObject {
		public static dynamic That (object subject) {
			return new Check(subject);
		}

		public static Result With(object subject, params Func<dynamic, Result>[] cases)
		{
			if (subject is IEnumerable<object>)
			{
				return WithStems(((IEnumerable<object>)subject).Cast<Check>(), cases);
			}
			if (subject is Check)
			{
				return WithStem((Check)subject, cases);
			}
			return WithSubject(subject, cases);
		}

		#region group cases
		static Result WithSubject(object subject, params Func<dynamic, Result>[] cases)
		{
			var result = new Result();
			foreach (var check in cases)
			{
				result.Merge(check(That(subject)));
			}
			return result;
		}

		static Result WithStems(IEnumerable<Check> subjects, params Func<dynamic, Result>[] cases)
		{
			var result = new Result();
			foreach (var subject in subjects)
			{
				foreach (var check in cases)
				{
					result.Merge(check(subject));
				}
			}
			return result;
		}

		static Result WithStem(Check subject, params Func<dynamic, Result>[] cases)
		{
			var result = new Result();
			foreach (var check in cases)
			{
				result.Merge(check(subject));
			}
			return result;
		}
		#endregion

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
					result = Add(ChainStep.Complex(binder.Name, args[0].ToString(), 0, (INamedPredicate)args[1]));
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
			
			var predicates = AllConstraintsAsPredicates(indexes).ToList();

			result.Target = subject;
			string pathSoFar = subject.GetType().Name;
			result.ValueChecked = WalkObjectTree(chain, ref pathSoFar, result, predicates);
			result.Path = pathSoFar;

			return true;
		}

		static object WalkObjectTree(List<ChainStep> chain, ref string path, Result result, IList<INamedPredicate> predicates)
		{
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
					return null;
				}

				if (next == null) {
					result.FailBecause(pathHere + " is null or not accessible");
					return null;
				}

				if (next is IEnumerable)
				{
					string stepMsg;
					var container = FilterWithNamedPredicate((IEnumerable)next, step, out stepMsg);
					pathHere += stepMsg;
					switch (step.ListAssertionType)
					{
						case ListAssertion.Single:
						case ListAssertion.Simple:
							if ( ! StepSingle(result, pathHere, container, out next)) return null;
							break;

						case ListAssertion.Index:
							if ( ! StepIndex(result, step, container, out next, ref pathHere)) return null;
							break;

						case ListAssertion.All:
							CheckAllSubpaths(result, predicates, pathHere, remainingChain, container);
							return null;

						case ListAssertion.Any:
							CheckAnySubpaths(result, predicates, remainingChain, pathHere, container);
							return null;

						default: throw new Exception("Unexpected list assertion type");
					}
				}

				result.Target = next;
				path = pathHere;
			}

			return ApplyPredicatesToEndOfChain(ref path, result, predicates, remainingChain);
		}

		static bool StepIndex(Result result, ChainStep step, object[] container, out object next, ref string pathHere)
		{
			next = null;
			if (step.SingleIndex < 0 || step.SingleIndex >= container.Length)
			{
				result.FailBecause(pathHere + " has length of " + container.Length + ", tried to access index " + step.SingleIndex);
				return false;
			}
			pathHere += "[" + step.SingleIndex + "]";
			next = container[step.SingleIndex];
			return true;
		}

		static bool StepSingle(Result result, string pathHere, object[] container, out object resultTarget)
		{
			resultTarget = null;
			if (container.Length > 1)
			{
				result.FailBecause(pathHere + " has length of " + container.Length + ", expected 1");
				return false;
			}
			if (container.Length < 1)
			{
				result.FailBecause(pathHere + " has no items");
				return false;
			}
			resultTarget = container[0];
			return true;
		}

		static object ApplyPredicatesToEndOfChain(ref string path, Result result, IList<INamedPredicate> predicates, List<ChainStep> remainingChain)
		{
			ChainStep step;
			if (remainingChain.Count == 1)
			{
				step = remainingChain[0];
				try
				{
					result.Target = result.Target.Get(step.Name);
					path += "." + step.Name;
				}
				catch (FastFailureException)
				{
					result.FailBecause(path + "." + step.Name + " is not a valid path");
					return null;
				}
			} else
			{
				step = ChainStep.SimpleStep("?");
			}

			if (result.Target is IEnumerable)
			{
				ApplyPredicatesToTerminalEnumerable(path, result, predicates, step);
			}
			else
			{
				ApplyPredicatesToSimpleTerminal(path, result, predicates);
			}
			return result.Target;
		}

		static void ApplyPredicatesToTerminalEnumerable(string path, Result result, IList<INamedPredicate> predicates, ChainStep step)
		{
			string stepMsg;
			var container = FilterWithNamedPredicate((IEnumerable)result.Target, step, out stepMsg);
			path += stepMsg;
			object target;
			switch (step.ListAssertionType)
			{
				case ListAssertion.Simple:
					ApplyPredicatesToSimpleTerminal(path, result, predicates);
					break;

				case ListAssertion.Single:
					if (!StepSingle(result, path, container, out target)) return;
					var singleResult = new Result{Target = target};
					ApplyPredicatesToSimpleTerminal(path, singleResult, predicates);
					result.Merge(singleResult);
					break;

				case ListAssertion.Index:
					if (!StepIndex(result, step, container, out target, ref path)) return;
					var indexResult = new Result{Target = target};
					ApplyPredicatesToSimpleTerminal(path, indexResult, predicates);
					result.Merge(indexResult);
					break;

				case ListAssertion.All:
					CheckAll(result, predicates, path, container);
					return;

				case ListAssertion.Any:
					CheckAny(result, predicates, path, container);
					return;

				default:
					throw new Exception("Unexpected list assertion type");
			}
		}

		
		static void ApplyPredicatesToSimpleTerminal(string path, Result result, IEnumerable<INamedPredicate> predicates)
		{
			foreach (var predicate in predicates)
			{
				string message;
				if (!predicate.Matches(result.Target, out message)) result.FailBecause(path + " " + message);
			}
		}
		static void CheckAny(Result result, IList<INamedPredicate> predicates, string pathHere, IEnumerable<object>  container)
		{
			var successCount = 0;
			var subResult = new Result();
			foreach (var route in container)
			{
				var cleanResult = new Result {Target = route};
				var localPath = pathHere;

				ApplyPredicatesToSimpleTerminal(localPath, cleanResult, predicates);

				subResult.Merge(cleanResult);
				if (cleanResult.Success) successCount++;
			}
			if (successCount < 1)
			{
				result.Merge(subResult);
			}
		}
		static void CheckAnySubpaths(Result result, IList<INamedPredicate> predicates, List<ChainStep> remainingChain, string pathHere, IEnumerable<object> container)
		{
			var successCount = 0;
			var subResult = new Result();
			foreach (var route in container)
			{
				var cleanResult = new Result {Target = route};
				var localPath = pathHere;
				WalkObjectTree(remainingChain, ref localPath, cleanResult, predicates);
				subResult.Merge(cleanResult);
				if (cleanResult.Success) successCount++;
			}
			if (successCount < 1)
			{
				result.Merge(subResult);
			}
		}
		
		static void CheckAll(Result result, IList<INamedPredicate> predicates, string pathHere, IEnumerable<object> container)
		{
			var i = 0;
			foreach (var route in container)
			{
				var cleanResult = new Result {Target = route};
				var localPath = pathHere + "[" + i + "]";
				ApplyPredicatesToSimpleTerminal(localPath, cleanResult, predicates);
				result.Merge(cleanResult);
				i++;
			}
		}
		static void CheckAllSubpaths(Result result, IList<INamedPredicate> predicates, string pathHere, List<ChainStep> remainingChain, IEnumerable<object> container)
		{
			var i = 0;
			foreach (var route in container)
			{
				var cleanResult = new Result {Target = route};
				var localPath = pathHere + "[" + i + "]";
				WalkObjectTree(remainingChain, ref localPath, cleanResult, predicates);
				result.Merge(cleanResult);
				i++;
			}
		}

		static object[] FilterWithNamedPredicate(IEnumerable source, ChainStep step, out string message)
		{
			string stepMsg = "";
			var container = source.Cast<object>().Where(o => step.FilterPredicate.Matches(o, out stepMsg)).ToArray();

			message = (string.IsNullOrEmpty(stepMsg))
				? ""
				: "(Matching: " + stepMsg + ")";

			return container;
		}

		static IEnumerable<INamedPredicate> AllConstraintsAsPredicates(IEnumerable<object> indexes)
		{
			var constraintsAsPredicates = new List<INamedPredicate>();
			foreach (var index in indexes)
			{
				if (index is INamedPredicate)
				{
					constraintsAsPredicates.Add((INamedPredicate)index);
				}
				else
				{
					throw new ArgumentException("All assertions must be INamedPredicate");
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

			/// <summary>
			/// Value at end of chain. This is the value that gets checked.
			/// Null if there was a failure in the chain
			/// </summary>
			public object ValueChecked { get; set; }

			/// <summary> The entire requested path </summary>
			public string Path { get; set; }

			public void Merge(Result otherResult) {
				Success &= otherResult.Success;
				Reasons = Reasons.Union(otherResult.Reasons).Distinct().ToList();
			}
		}
	}
}