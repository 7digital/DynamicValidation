using System;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using NUnit.Framework;

namespace DynamicValidation {
	public class Check : DynamicObject {
		readonly object subject;
		readonly List<string> chain;

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

		public static dynamic That (object subject) {
			return new Check(subject);
		}

		public override bool TryGetMember (GetMemberBinder binder, out object result) {
			Console.WriteLine("dot - " + binder.Name);
			result = Add(binder.Name);
			return true;
		}

		public override bool TryGetIndex (GetIndexBinder binder, object[] indexes, out object result) {
			Console.WriteLine(string.Join(":",binder.CallInfo.ArgumentNames));
			Console.WriteLine("[" + string.Join(" ", indexes) + "]");
			result = string.Join(" ", chain) + " " + string.Join(" ", indexes);
			return true;
		}

		public class Result {
			/// <summary> True if all predicates passed. False if any failed  </summary>
			public bool Success { get; set; }

			/// <summary> Description of failures if not success. Undefined otherwise. </summary>
			public IEnumerable<string> Reasons { get; set; }

			/// <summary> Final object tested. If null is encountered before tested object, this will be null </summary>
			public object Target { get; set; }
		}
	}
}