# Analyzers to Create

| Id | Analyzer | Status | Description |
|----------|----------|--------|-------------|
| #1 | Missing Equality Comparer | ✔️ | For various LINQ methods (ToDictionary etc.), check if the underlying type of TKey implements IEquatable<T> as well as overrides GetHashCode(). |
| #2 |  Logger parameter should be first parameter | 🔵 | In constructors, parameters of type `ILogger` or `ILogger<T>` should be the first or last parameter. This should be configurable. Default is last. |
| #3 |  Namespace using order | 🔵 | Namespace usings should be ordered by System, System.* and then all the other namespaces. Also provide a code-fix feature to automatically fix it. |
| #4 |  Member Ordering | 🔵 | Ensure type members are ordered like (default): const, static fields, readonly fields, fields, properties, events, delegates, static constructors, public constructors, other constructors, public methods, protected methods, internal methods, private methods, types. Order should be configurable through `.globalconfig`  file. |
| #5 |  Object not disposed | 🔵 | There are already analyzers to check that for direct object creation, the object is being disposed in all execution paths like: `using var stream = new MemoryStream(...)`. However, if a disposable object is returned from a method (e.g. `sqlConnection.CreateCommand()`), these analyzers don't check for that. Therefore, it would be good to have such an analyzer. It should also be configurable through `.globalconfig` which types to ignore (e.g. `System.Threading.Tasks.Task`) as well as which methods to ignore. |
| #6 |  String interpolation is not necessary | 🔵 | Check for interpolated string where interpolation is not  required. e.g.: `var whatever = $"bla bla";`. |
| #7 |  Verbatim string is not necessary | 🔵 | Sometimes, verbatim string literals are used but are not required. e.g.: `const string whatever = @"bla bla";`. |
| #8 |  Public methods should not return materialized collections as enumerables  | 🔵 | By returning meterialized collections as `IEnumerable`, `IEnumerable<T>`, `Task<IEnumerable>` or `Task<IEnumerable<T>>`, the caller most likely will re-materialize it by calling `ToList()` or similar methods which is an overhead. Instead, return a collection abstraction like `IReadOnlyCollection<T>` or `IReadOnlyList<T>`. |
| #9 |  Awaiting `Task.CompletedTask` in an async method is not necessary | 🔵 | Either remove `await Task.CompletedTask` or replace it with `return Task.CompletedTask`. |
| #10 | Awaiting `Task.FromResult` in an async method is not necessary | 🔵 | Either remove `await Task.FromResult` or replace it with `return Task.FromResult`. |
| #11 | Don't inject untyped logger (`ILogger`) in construcotrs | 🔵 | Instead, use a typed logger `ILogger<TContext>`. |
| #12 | Incorrect type parameter used for `ILogger<TContext>` | 🔵 | When injecting `ILogger<TContext>` in constructors, the type argument should be the type of the class it is injected to. |
| #13 | Pragma warning suppression not restored | 🔵 | The `#pragma warning disable X` is not restored. |
| #14 | Pragma warning suppression not restored in correct order of suppression | 🔵 | When suppressing warnings, the warning suppression restoration statement must be in the reverse order. |
| #15 | Do not use general warning suppression | 🔵 | When suppressing warnings through `#pragma warning disable` a diagnostic ID must be provided all the time. Disallow general warning suppression.  |

# Other Open Points
| Summary | Status | Description |
|---------|--------|-------------|
| Build pipeline and upload to NuGet server. | 🔵 | |
| Unified settings handling | 🔵 | Create an easy way to retrieve the config for every analyzer diagnostic separately. |
| Logging | 🔵 | Create a simple logger which logs to a temporary directory. Since analyzers can run in parallel, file access need to be synchronized (e.g. throgh a named mutex). Logging should only be used to log fatal errors. |
