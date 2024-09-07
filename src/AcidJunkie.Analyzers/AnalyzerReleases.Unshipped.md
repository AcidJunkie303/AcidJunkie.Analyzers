; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID  | Category | Severity | Notes
---------|----------|----------|-------
BJZZ1001 | Design   | Error    | The class contain multiple methods which accept one parameter of type SqlDataReader and have the same return type
BJZZ1002 | Design   | Error    | The name of the SqlParameter must start with @
