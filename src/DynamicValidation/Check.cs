using System;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;

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
			public bool Success { get; set; }
			public IEnumerable<string> Reasons { get; set; }
		}
	}
}