using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AcidJunkie.Analyzers.Extensions;

public static class LocationExtensions
{
    public static Location CreateLocationSpan(this SyntaxTree syntaxTree, Location startLocation, Location endLocation)
    {
        var start = Math.Min(startLocation.SourceSpan.Start, endLocation.SourceSpan.Start);
        var end = Math.Max(startLocation.SourceSpan.End, endLocation.SourceSpan.End);

        var textSpan = TextSpan.FromBounds(start, end);

        return Location.Create(syntaxTree, textSpan);
    }
}
