Console.WriteLine("Hello, World!");

Console.WriteLine(GetNumbers());

static IEnumerable<int> GetNumbers() => Enumerable.Range(1, 10).ToList();
