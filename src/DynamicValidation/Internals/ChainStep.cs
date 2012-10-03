using System;

namespace DynamicValidation
{
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
				case "any": return ListAssertion.Any;
				case "all": return ListAssertion.All;
				case "single": return ListAssertion.Single;
				case "index": return ListAssertion.Index;
				default: return ListAssertion.Single;
			}
		}

		public static bool Anything(object arg) { return true; }
	}
}