using Microsoft.Extensions.Logging;

namespace Playground;

internal sealed class MyService
{
    private readonly ILogger<MyService> _logger;
    private int _field;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public void DoSomething() => _logger.Log(LogLevel.Information, "Hello, World!");

    public void DoSomethingElse()
    {
        var items = Enumerable.Range(0, 10).Select(x =>
        {
            Console.WriteLine(x);
            return Enumerable.Range(0, 10).Select(x => x);
        }).ToList();

        var _field = 303;
        Console.WriteLine(_field);
    }
}
