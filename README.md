# AcidJunkie.Analyzers

C# code analyzers for .NET 8.0 and higher. Lower .NET versions are not supported.

Source: https://github.com/AcidJunkie303/AcidJunkie.Analyzers

# Rules

| Id                                                                                             | Category                | Technology       | Severity | Has Code fix | Title                                                                       |
|:-----------------------------------------------------------------------------------------------|:------------------------|:-----------------|:--------:|:------------:|:----------------------------------------------------------------------------|
| [AJ0001](https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0001.md) | Predictability          | General          |    ⚠️    |      ❌       | Provide an equality comparer argument                                       | 
| [AJ0002](https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0002.md) | Intention / Performance | Entity Framework |    ⚠️    |      ❌       | Always specify the tracking type when using Entity Framework                | 
| [AJ0003](https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0003.md) | Performance             | General          |    ⚠️    |      ❌       | Do not return materialized collection as enumerable                         | 
| [AJ0004](https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0004.md) | Performance             | General          |    ⚠️    |      ❌       | Do not create tasks of enumerable type containing a materialized collection | 
| [AJ0005](https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0005.md) | Style                   | General          |    ⚠️    |      ❌       | Do not use general warning suppression                                      | 
| [AJ0006](https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0006.md) | Style                   | General          |    ⚠️    |      ❌       | Classes containing extension methods should have an `Extensions` suffix     | 
| [AJ0007](https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0007.md) | Style                   | General          |    ⚠️    |      ❌       | Non-compliant parameter order                                               | 
| [AJ9999](https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ9999.md) | Analyzer Error          | General          |    ⚠️    |      ❌       | Unexpected error in AcidJunkie.Analyzers                                    | 

# Logging

To enable logging, set the following property to true in the `.editorconfig` file:

```
[*.cs]
AcidJunkie_Analyzers.is_logging_enabled = true
```

Please be aware that logging will slow down the analysis by several factors. It should only be used for debugging
purposes.
