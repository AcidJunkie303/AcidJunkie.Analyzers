; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

Rule ID | Category       | Severity    | Notes
--------|----------------|-------------|-------
AJ0001  | Predictability | Warning     | Missing using statement -> https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/7-add-documentation-for-all-existing-analyzers/src/AcidJunkie.Analyzers/Diagnosers/MissingEqualityComparer/MissingEqualityComparerAnalyzer.cs
AJ0002  | Reliability    | Warning     | Missing using statement -> https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/7-add-documentation-for-all-existing-analyzers/src/AcidJunkie.Analyzers/Diagnosers/MissingUsingStatement/MissingUsingStatementAnalyzer.cs
AJ0003  | Performance    | Warning     | Do not return materialised collection as enumerable -> https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/7-add-documentation-for-all-existing-analyzers/src/AcidJunkie.Analyzers/Diagnosers/ReturnMaterialisedCollectionAsEnumerable/ReturnMaterialisedCollectionAsEnumerableAnalyzer.cs
AJ0004  | Performance    | Warning     | Do not create tasks of enumerable type containing a materialised collection -> https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/7-add-documentation-for-all-existing-analyzers/src/AcidJunkie.Analyzers/Diagnosers/ReturnMaterialisedCollectionAsEnumerable/TaskCreationWithMaterialisedCollectionAsEnumerableAnalyzer.cs
AJ0005  | Code Smell     | Warning     | Do not use general warning suppression -> https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/7-add-documentation-for-all-existing-analyzers/src/AcidJunkie.Analyzers/Diagnosers/WarningSuppression/GeneralWarningSuppressionAnalyzer.cs
AJ9999  | Error          | Warning       | An error occurred in the AcidJunkie.Analyzers package. Check the log file 'AJ.Analyzers.log' in the temp folder
