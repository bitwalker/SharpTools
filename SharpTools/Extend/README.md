## SharpTools.Extend

This module provides a large number of helpful extensions to the standard .NET library.

### Collections

These are extensions to both the standard .NET collections, as well as those collections
present in the SharpTools.Collections module.

#### GenericComparer

Provides a way to generate `IComparer/IComparer<T>` implementations on the fly for any type,
with control over directionality of the comparison (i.e. ascending/descending order).

#### MultiComparer

Provides a way to combine multiple `IComparer/IComparer<T>` into a single comparer which applies
comparisons in order of priority (parametric order). This allows you to perform complex comparisons such
as "sort this list of orders by date created (oldest first/ascending order), but if they were created the
same day, sort them by order status (priority/descending order)". A very useful and powerful tool
to have at your disposal.