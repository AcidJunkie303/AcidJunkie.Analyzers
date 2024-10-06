using System.Runtime.CompilerServices;

namespace AcidJunkie.Analyzers.Logging;

internal sealed class NullLogger<TContext> : ILogger<TContext>
    where TContext : class
{
    public void WriteLine(Func<string> messageFactory, [CallerMemberName] string memberName = "")
    {
    }

    public static NullLogger<TContext> Default { get; } = new();
}
