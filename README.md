DynamicValidation
=================

Object-Tree validation tools.

Experiments in the direction of simple and readable validation of the
structure and data of any object tree.

Validation assertions can be either INamedPredicates or NUnit assertion
constraints.

Todo
----
* Maybe handle recursive data structures by having another List assertion strategy ("all,recurse"?)
  will assert on matching level, and any children with same name.
  (this would allow for `group->group->group...->item`)

Example
-------

```
Check.Result result = Check.That(MyObject).Identifier.LocalIdentifier[Should.NotBeNull];

if ( ! result.Success) throw new Exception(result.Reason);
```

The idea is to get the check object, walk the object tree
then pass a set of validation predicates into the index.

Then get a result back, success or failure, with a message
for failure which is the name of the predicates that failed.

If any part of the object tree is null except the last item
then that's a failure with it's own message.

To check each item in an enumerable, you can use
```
var result = Check.That(subject).container("all")[Should.NotBeNull)];
```

If the validation needs to check children of an enumerable
can define like this:
```
    Check.That(thing).a.b[Should.HaveAtLeast(1)]
    Check.That(thing).a.b("all").c[Should.NotBeEmpty]
    Check.That(thing).a.b("first").c.d[Should.NotBeEmpty]
    Check.That(thing).a.b("single").c.d[Should.NotBeEmpty]
```
