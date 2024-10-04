# Analyzers to Create

| Analyzer | Status | Description |
|----------|--------|-------------|
| Missing Equality Comparer | ✔️ | For various LINQ methods (ToDictionary etc.), check if the underlying type of TKey implements IEquatable<T> as well as overrides GetHashCode(). |
| Logger parameter should be first parameter | 🔵 | In constructors, parameters of type `ILogger` or `ILogger<T>` should be the first parameter. |
| Namespace using order | 🔵 | Namespace usings should be ordered by System, System.* and then all the other namespaces. Also provide a code-fix feature to automatically fix it. |
| Member Ordering | 🔵 | Ensure type members are ordered like (default): const, static fields, readonly fields, fields, properties, events, delegates, static constructors, public constructors, other constructors, public methods, protected methods, internal methods, private methods, types. Order should be configurable through `.globalconfig`  file. |
| Object not disposed | 🔵 | There are already analyzers to check that for direct object creation, the object is being disposed in all execution paths like: `using var stream = new MemoryStream(...)`. However, if a disposable object is returned from a method (e.g. `sqlConnection.CreateCommand()`), these analyzers don't check for that. Therefore, it would be good to have such an analyzer. It should also be configurable through `.globalconfig` which types to ignore (e.g. `System.Threading.Tasks.Task`) as well as which methods to ignore. |
| String interpolation is not necessary | 🔵 | Check for interpolated string where interpolation is not  required. e.g.: `var whatever = $"bla bla";`. |
| Verbatim string is not necessary | 🔵 | Sometimes, verbatim string literals are used but are not required. e.g.: `const string whatever = @"bla bla";`. |
| Awaiting completed task not necessary (1) | 🔵 | In async methods, where there is already an await statement, the following code would raise this warning: `await Task.CompletedTask`. Same applies to `await Task.FromResult()`. |
| Awaiting completed task not necessary (2) | 🔵 | In async methods, where the only statatement with the `await` keyword is `await Task.CompletedTask`, raise a suggestion to remove the await keyword and return `Task.CompletedTask`. Same applies to `await Task.FromResult()`. |
| Public methods should not return materialized collections as enumerables  | 🔵 | By returning meterialized collections as `IEnumerable`, `IEnumerable<T>`, `Task<IEnumerable>` or `Task<IEnumerable<T>>`, the caller most likely will re-materialize it by calling `ToList()` or similar methods which is an overhead. Instead, return a collection abstraction like `IReadOnlyCollection<T>` or `IReadOnlyList<T>`. |
| Awaiting `Task.CompletedTask` in an async method is not necessary | 🔵 | Either remove `await Task.CompletedTask` or replace it with `return Task.CompletedTask`. |
| Awaiting `Task.FromResult` in an async method is not necessary | 🔵 | Either remove `await Task.FromResult` or replace it with `return Task.FromResult`. |
| Don't inject untyped logger (`ILogger`) in construcotrs | 🔵 | Instead, use a typed logger `ILogger<TContext>`. |
| Incorrect type parameter used for `ILogger<TContext>` | 🔵 | When injecting `ILogger<TContext>` in constructors, the type argument should be the type of the class it is injected to. |
| The `ILogger<TContext>` parameter should be either the first or the last parameter |  When injecting `ILogger<TContext>` it must be either the first or the last parameter (default first). Provide configuration. |
| Pragma warning suppression not restored | 🔵 | The `#pragma warning disable X` is not restored. |
| Pragma warning suppression not restored in correct order of suppression | 🔵 | When suppressing warnings, the warning suppression restoration statement must be in the reverse order. |
| Do not use general warning suppression | 🔵 | When suppressing warnings through `#pragma warning disable` a diagnostic ID must be provided all the time. Disallow general warning suppression.  |

# Other Open Points
| Summary | Status | Description |
|---------|--------|-------------|
| Build pipeline and upload to NuGet server. | 🔵 | |
| Unified settings handling | 🔵 | Create an easy way to retrieve the config for every analyzer diagnostic separately. |
| Logging | 🔵 | Create a simple logger which logs to a temporary directory. Since analyzers can run in parallel, file access need to be synchronized (e.g. throgh a named mutex). Logging should only be used to log fatal errors. |
