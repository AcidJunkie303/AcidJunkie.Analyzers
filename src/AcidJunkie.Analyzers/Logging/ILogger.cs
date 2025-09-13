using System.Diagnostics.CodeAnalysis;

namespace AcidJunkie.Analyzers.Logging;

[SuppressMessage("Clean Code", "S2326:'{0}' is not used in the interface.", Justification = "Types which implement this interface will need it")]
internal interface ILogger<TContext>
    where TContext : class
{
    LogLevel LogLevel { get; }
    bool IsLoggingEnabled { get; }
}
