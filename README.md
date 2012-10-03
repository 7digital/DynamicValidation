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
