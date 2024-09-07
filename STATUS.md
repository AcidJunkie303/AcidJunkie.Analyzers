# Analyzers to Create

- Namespace Using Ordering Analyzer
- Type Member Ordering Analyzer
- Missing IEqualityComparer Argument

# Analyzers

## Namespace Using Ordering Analyzer
Ensures that the namespace using directives are ordered accodring to rules.
These rules can be configured in a .globalconfig file.

## Type Member Ordering Analyzer
Ensures that type members (fields, properties, methods, constructors) appear in a configurable order.

## Missing IEqualityComparer Argument
Ensures that method invocations, dictionary or hash-set creation use a compatible IEqualityComparer parameter in the following cases:
- The type used for the key does not implement IEquatable<T> and/or GetHashCode() is not implemented.
- The type used for a HashSet<T> does not implement IEquatable<T> and/or GetHashCode() is not implemented. 
