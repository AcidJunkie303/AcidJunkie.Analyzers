# AcidJunkie.Analyzers

C# code analyzers for .NET 8.0 and higher. Lower .NET versions are not supported.

Source: https://github.com/AcidJunkie303/AcidJunkie.Analyzers

# Rules

| Id                                                                                             | Category                | Technology       | Severity | Has Code fix | Title                                                                            |
|:-----------------------------------------------------------------------------------------------|:------------------------|:-----------------|:--------:|:------------:|:---------------------------------------------------------------------------------|
| [AJ0001](https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0001.md) | Predictability          | General          |    ⚠️    |      ❌       | Provide an equality comparer argument                                            | 
| [AJ0002](https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0002.md) | Intention / Performance | Entity Framework |    ⚠️    |      ❌       | Always specify the tracking type when using Entity Framework                     | 
| [AJ0003](https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0003.md) | Performance             | General          |    ⚠️    |      ❌       | Do not return materialized collection as enumerable                              | 
| [AJ0004](https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0004.md) | Performance             | General          |    ⚠️    |      ❌       | Do not create tasks of enumerable type containing a materialized collection      | 
| [AJ0005](https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0005.md) | Style                   | General          |    ⚠️    |      ❌       | Do not use general warning suppression                                           | 
| [AJ0006](https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0006.md) | Style                   | General          |    ⚠️    |      ❌       | Classes containing extension methods should have an `Extensions` suffix          | 
| [AJ0007](https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0007.md) | Style                   | General          |    ⚠️    |      ❌       | Non-compliant parameter order                                                    | 
| [AJ0008](https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0008.md) | Performance             | General          |    ⚠️    |      ❌       | Do not await Task.FromResult()                                                   | 
| [AJ0009](https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0009.md) | Readability / Clarity   | General          |    ⚠️    |      ❌       | Lambda variable declaration hides outer lambda variable that share the same name | 
| [AJ0010](https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0010.md) | Readability             | General          |    ⚠️    |      ❌       | Declare constants at the top of the method                                       | 
| [AJ0011](https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0010.md) | Intention               | General          |    ⚠️    |      ✅       | Use is or is not for null-comparison                                             | 
| [AJ9999](https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ9999.md) | Analyzer Error          | General          |    ⚠️    |      ❌       | Unexpected error in AcidJunkie.Analyzers                                         | 

# Logging

To enable logging, set the following property to true in the `.editorconfig` file:

```
[*.cs]
AcidJunkie_Analyzers.is_logging_enabled = None | Duration | Full
```

| Value    | Description                                                       |
|:---------|:------------------------------------------------------------------|
| None     | Logging is disabled                                               |  
| Duration | Only log the duration it took for each analyzer to analyze a node |
| Full     | Full logging                                                      |

Please be aware that logging will slow down the analysis by several factors. It should only be used for debugging
purposes.
