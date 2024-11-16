namespace Playground;

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("Hello, World!");

        var service = new MyService(null!);
        service.DoSomething();
    }
}
