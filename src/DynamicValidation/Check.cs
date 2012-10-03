using System;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using Mono.Reflection;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace DynamicValidation {
	public class Check : DynamicObject {
		readonly object subject;
		readonly List<string> chain;

		public static dynamic That (object subject) {
			return new Check(subject);
		}


		Check (object subject) {
			this.subject = subject;
			chain = new List<string>();
		}

		Check (object subject, IEnumerable<string> oldChain, string nextItem) {
			this.subject = subject;
			chain = new List<string>(oldChain) { nextItem };
		}

		dynamic Add (string name) {
			return new Check(subject, chain, name);
		}


		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			result = Add(binder.Name);
			return true;
		}

		public override bool TryGetMember (GetMemberBinder binder, out object result) {
			// No actual access here, just record the request.
			result = Add(binder.Name);
			return true;
		}

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

			foreach (var name in chain)
			{
				if (currentTarget == null)
				{
					result.FailBecause(pathSoFar + " is null or can't be accessed");
					return;
				}

				var definitionCount  = currentTarget.CountDefinitions(name);

				if (definitionCount < 1)
				{
					result.FailBecause(pathSoFar + "." + name + " is not a possible path");
					return;
				}
				if (definitionCount > 1)
				{
					result.FailBecause(pathSoFar + "." + name + " is ambiguous");
					return;
				}

				pathSoFar += "."+name;
				currentTarget = currentTarget.Get(name);
			}

			result.Target = currentTarget;
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

	public static class TypeExtensions
	{
		public static int CountDefinitions(this object target, string memberName)
		{
			if (target == null) return 0;
			var type = target.GetType();

			var fieldNames = type.GetFields().Select(f=>f.Name);
			var propertyNames = type.GetProperties().Select(p=>p.Name);

			return fieldNames.Count(name=>name == memberName)
				+ propertyNames.Count(name=>name == memberName);
		}
		
		public static object Get(this object target, string memberName)
		{
			var field = target.GetType().GetField(memberName);
			if (field == null)
			{
				var prop = target.GetType().GetProperty(memberName);
				field = prop.GetBackingField();
			}

			if (field == null) return null;

			return field.GetValue(target);
		}
	}
}