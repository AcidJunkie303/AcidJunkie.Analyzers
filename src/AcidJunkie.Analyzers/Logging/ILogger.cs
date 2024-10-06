using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace AcidJunkie.Analyzers.Logging;

[SuppressMessage("Clean Code", "S2326:'{0}' is not used in the interface.", Justification = "Types which implemen this interface will need it")]
internal interface ILogger<TContext>
    where TContext : class
{
    void WriteLine(Func<string> messageFactory, [CallerMemberName] string memberName = "");
}
