DynamicValidation
=================

Object-Tree validation tools.

Experiments in the direction of simple and readable validation of the
structure and data of any object tree.

Validation assertions can be either INamedPredicates or NUnit assertion
constraints.

Todo
----
* Should not be sensitive about NUnit.Framework version (if possible) -- maybe reflect stuff out?
  Maybe remove IResoveConstraint stuff?

Example
-------

```
Check.Result result = Check.That(MyObject).Identifier.LocalIdentifier[Is.Not.Null];

if ( ! result.Success) throw new Exception(string.Join("\r\n",result.Reasons));
```

The idea is to get the check object, walk the object tree
then pass a set of validation predicates into the index.

Then get a result back, success or failure, with a message
for failure which is the name of the predicates that failed.

If any part of the object tree is null except the last item
then that's a failure with it's own message.

To check each item in an enumerable, you can use
```
var result = Check.That(subject).container[Should.AllMatch(Is.Not.Null)];
```

If the validation needs to check children of an enumerable
should be able to define like this:
```
    Check.That(thing).a.b[Should.HaveAtLeast(1)]
    Check.That(thing).a.b("all").c[Is.NotEmpty]
    Check.That(thing).a.b("first").c.d[Is.NotEmpty]
    Check.That(thing).a.b("single").c.d[Is.NotEmpty]
```
but it's not implemented yet.
