using FluentAssertions;

namespace AcidJunkie.Analyzers.Tests.Helpers.CodeParsing;

public sealed class TaggedSourceCodeParserTests
{
    [Fact]
    public void Test()
    {
        // arrange
        const string code = "aaa[|<?>XXX|]zzz";

        // act
        var result = TaggedSourceCodeParser.Parse(code);

        // assert
        result.PureCode.Should().Be("aaaXXXzzz");
        result.ExpectedDiagnostics.Should().HaveCount(1);
        result.ExpectedDiagnostics[0].Should().BeEquivalentTo(new ExpectedDiagnostic("?", 1, 4, 1, 7));
    }

    [Fact]
    public void Test2()
    {
        // arrange
        const string code = "aaa[|<?>XXX|]bbb[|<??>YYY|]ccc";

        // act
        var result = TaggedSourceCodeParser.Parse(code);

        // assert
        result.PureCode.Should().Be("aaaXXXbbbYYYccc");
        result.ExpectedDiagnostics.Should().HaveCount(2);
        result.ExpectedDiagnostics[0].Should().BeEquivalentTo(new ExpectedDiagnostic("?", 1, 4, 1, 7));
        result.ExpectedDiagnostics[1].Should().BeEquivalentTo(new ExpectedDiagnostic("??", 1, 10, 1, 13));
    }
}
