using Microsoft.EntityFrameworkCore;

namespace Playground;

internal static class Program
{
    private static async Task Main()
    {
        Console.WriteLine("Hello, World!");
        Console.WriteLine("Hello, World!");

        await using var ctx = new TestContext();

        var aa = await ctx.Entities.AsNoTracking().ToDictionaryAsync(a => a.Id, a => a.Name);
        var bb = await ctx.Entities.Where(a => a.Id > 1).ToListAsync();
        var cc = await ctx.Entities.Where(a => a.Id > 1).Select(x => x.Name).ToListAsync();
        var dd = await ctx.Entities.Where(a => a.Name == null).ToListAsync();

        var xxx = Enumerable.Range(0, 10)
                            .Select(a => Enumerable.Range(0, 10).Select(a => a))
                            .ToList();
        static IEnumerable<int> GetNumbers() => Enumerable.Range(1, 10).ToList();
    }

    private static string? GetStringValue() => null;
    private static int? GetNullableValue() => null;
}
