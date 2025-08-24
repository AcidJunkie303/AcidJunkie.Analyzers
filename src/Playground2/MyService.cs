using Microsoft.Extensions.Logging;

namespace Playground2;

internal sealed class MyService
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public void DoSomething() => _logger.Log(LogLevel.Information, "Hello, World!");
}
