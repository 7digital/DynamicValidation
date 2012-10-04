namespace DynamicValidation.Internals
{
	enum ListAssertion
	{
		Single = 0, // exactly 1 item (results in single sub-tree)
		All = 1,  // every case must pass (results in multiple sub-trees)
		Any = 2, // at least one passing case (results in multiple sub-trees)
		Index = 3, // a specific index (results in single sub-tree)
		Simple = 4 // default, either a non-enumerable or enumerable with a single item.
	}
}