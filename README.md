# ExpressionIssue


Issues with LINQ translation:
```
System.InvalidOperationException: The LINQ expression 'Where<TransparentIdentifier<Booking, Party>>(
    source: LeftJoin<Booking, Party, Nullable<int>, TransparentIdentifier<Booking, Party>>(
        outer: DbSet<Booking>, 
        inner: DbSet<Party>, 
        outerKeySelector: (b) => Property<Nullable<int>>(b, "PartyId"), 
        innerKeySelector: (p) => Property<Nullable<int>>(p, "Id"), 
        resultSelector: (o, i) => new TransparentIdentifier<Booking, Party>(
            Outer = o, 
            Inner = i
        )), 
    predicate: (b) => Invoke(p => p.Name.Contains("foo"), b.Inner[Party])
)' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to either AsEnumerable(), AsAsyncEnumerable(), ToList(), or ToListAsync(). See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
```

Specifically the use of 'Expression.Invoke' in the test 'ExpressionBasedQuery' breaks ONLY on .NET 3.0 version of entity framework core.
