using System.Text;

namespace AcidJunkie.Analyzers.Tests.Helpers.CodeParsing;

/*
internal static class TaggedSourceCodeParser_Old
{
    private static readonly Regex BeginTagExpression = new(@"\[\|<(?<tag>[A-Za-z0-9]+)>", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
    private static readonly Regex EndTagExpression = new(@"\|\]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

    private sealed record Line(string Text, int LineIndex, IReadOnlyList<MatchInfo> Matches);
    private sealed record MatchInfo(bool IsBeginTag, int LineIndex, int ColumnIndex, Match Match);



    public static ParserResult Parse(string code)
    {




        var matches = beginTagMatches.Concat(endTagMatches).OrderBy(a => a.)





        // Stack to manage nested tags
        Stack<(string TagName, int StartLineIndex, int StartCharIndex, StringBuilder Content)> tagStack = new();
        List<ExpectedDiagnostic> diagnostics = [];

        StringBuilder result = new();
        int lineIndex = 1, charIndex = 1;
        var insideTag = false;
        StringBuilder currentContent = new();
        var tagStart = TagStart.AsSpan();
        var tagEnd = TagEnd.AsSpan();

        var codeSpan = code.AsSpan();

        for (var i = 0; i < codeSpan.Length; i++)
        {
            var c = codeSpan[i];

            if (c == '\n')
            {
                lineIndex++;
                charIndex = 1;
                currentContent.Append(c);

                continue;
            }

            if (!insideTag && codeSpan.Slice(i).StartsWith(tagStart, StringComparison.Ordinal))
            {
                // Start of a new tag
                insideTag = true;

                // Extract the tag name
                var tagNameStart = i + TagStart.Length;
                var tagNameEnd = code.IndexOf('>', tagNameStart);
                var currentTagName = code.Substring(tagNameStart, tagNameEnd - tagNameStart);

                tagStack.Push((currentTagName, lineIndex, charIndex, currentContent));
                currentContent = new StringBuilder();
                i = tagNameEnd; // Move index to the end of the tag name
                charIndex += tagNameEnd - i + 1;
                continue;
            }

            if (insideTag && codeSpan.Slice(i).StartsWith(tagEnd, StringComparison.Ordinal))
            {
                // End of a tag
                insideTag = false;
                var (tagName, startLine, startChar, parentContent) = tagStack.Pop();
                var diagnostic = new ExpectedDiagnostic(tagName, startLine, startChar, lineIndex, charIndex + 1);
                diagnostics.Add(diagnostic);
                parentContent.Append(currentContent);
                currentContent = parentContent;
                i += 1; // Skip '|]'
                charIndex -= TagEnd.Length;
                continue;
            }

            currentContent.Append(c);
            charIndex++;
        }

        result.Append(currentContent);

        return new(result.ToString(), diagnostics);
    }

    private static IEnumerable<MatchInfo> Parse(string code)
    {
        var result = new List<MatchInfo>();

        var lines = code.Split('\n');




    }
    private static Line ParseLine(string line, int lineIndex)
    {
        var beginTagMatches = BeginTagExpression
            .Matches(line)
            .Select(match => new MatchInfo(true, lineIndex, match.Index + 1, match));

        var endTagMatches = BeginTagExpression
            .Matches(line)
            .Select(match => new MatchInfo(false, lineIndex, match.Index + 1, match));

        var matches = beginTagMatches.Concat(endTagMatches).OrderBy(a => a.ColumnIndex).ToList();

        return new(line, )
    }

}
*/
internal static class TaggedSourceCodeParser
{
    private const string TagStart = "[|<";
    private const string TagEnd = "|]";

    public static ParserResult Parse(string code)
    {
        // Stack to manage nested tags
        Stack<(string TagName, int StartLineIndex, int StartCharIndex, StringBuilder Content)> tagStack = new();
        List<ExpectedDiagnostic> diagnostics = [];

        StringBuilder result = new();
        int lineIndex = 1, charIndex = 1;
        var insideTag = false;
        StringBuilder currentContent = new();
        var tagStart = TagStart.AsSpan();
        var tagEnd = TagEnd.AsSpan();

        var codeSpan = code.AsSpan();

        for (var i = 0; i < codeSpan.Length; i++)
        {
            var c = codeSpan[i];

            if (c == '\n')
            {
                lineIndex++;
                charIndex = 1;
                currentContent.Append(c);

                continue;
            }

            if (!insideTag && codeSpan.Slice(i).StartsWith(tagStart, StringComparison.Ordinal))
            {
                // Start of a new tag
                insideTag = true;

                // Extract the tag name
                var tagNameStart = i + TagStart.Length;
                var tagNameEnd = code.IndexOf('>', tagNameStart);
                var currentTagName = code.Substring(tagNameStart, tagNameEnd - tagNameStart);

                tagStack.Push((currentTagName, lineIndex, charIndex, currentContent));
                currentContent = new StringBuilder();
                i = tagNameEnd; // Move index to the end of the tag name
                //charIndex += tagNameEnd - i + 1;
                //charIndex -= tagNameEnd - tagNameStart;
                continue;
            }

            if (insideTag && codeSpan.Slice(i).StartsWith(tagEnd, StringComparison.Ordinal))
            {
                // End of a tag
                insideTag = false;
                var (tagName, startLine, startChar, parentContent) = tagStack.Pop();
                var diagnostic = new ExpectedDiagnostic(tagName, startLine, startChar, lineIndex, charIndex);
                diagnostics.Add(diagnostic);
                parentContent.Append(currentContent);
                currentContent = parentContent;
                i += 1; // Skip '|]'
                //charIndex -= TagEnd.Length;
                continue;
            }

            currentContent.Append(c);
            charIndex++;
        }

        result.Append(currentContent);

        return new(result.ToString(), diagnostics);
    }
}
