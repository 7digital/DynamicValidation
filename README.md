DynamicValidation
=================

Object-Tree validation tools.

Experiments in the direction of simple and readable validation of the
structure and data of any object tree.

Validation assertions can be either INamedPredicates or NUnit assertion
constraints.

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

Not entirely sure how to handle lists/enumerations. Probably
something like
```
    Check.That(thing).a.b(Has.All)[...]
```
If the validation needs to check both aspect of an enumerable
and it's contents, should split the tests up.
```
    Check.That(thing).a.b[Has.AtLeastOne]
    Check.That(thing).a.b("all").c[Is.NotEmpty]
    Check.That(thing).a.b("first").c.d[Is.NotEmpty]
    Check.That(thing).a.b("single").c.d[Is.NotEmpty]
```
or something like that.
