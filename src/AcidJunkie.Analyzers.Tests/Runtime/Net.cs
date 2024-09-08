using Microsoft.CodeAnalysis.Testing;

namespace AcidJunkie.Analyzers.Tests.Runtime;

internal static class Net
{
    internal static class Assemblies
    {
        private static readonly Lazy<ReferenceAssemblies> LazyNet60 = new(() =>
            new ReferenceAssemblies(
                "net6.0",
                new PackageIdentity(
                    "Microsoft.NETCore.App.Ref",
                    "6.0.29"),
                Path.Combine("ref", "net6.0"))
            .WithPackages([new PackageIdentity("Microsoft.Bcl.AsyncInterfaces", "1.0.0.0")])
        );

        private static readonly Lazy<ReferenceAssemblies> LazyNet70 = new(() =>
            new ReferenceAssemblies(
                    "net7.0",
                    new PackageIdentity(
                        "Microsoft.NETCore.App.Ref",
                        "7.0.16"),
                    Path.Combine("ref", "net6.0"))
                .WithPackages([new PackageIdentity("Microsoft.Bcl.AsyncInterfaces", "1.0.0.0")])
        );

        private static readonly Lazy<ReferenceAssemblies> LazyNet80 = new(() =>
            new ReferenceAssemblies(
                "net8.0",
                new PackageIdentity(
                    "Microsoft.NETCore.App.Ref",
                    "8.0.4"),
                Path.Combine("ref", "net8.0"))
            .WithPackages([new PackageIdentity("Microsoft.Bcl.AsyncInterfaces", "1.0.0.0")])
        );

        public static ReferenceAssemblies Net60 => LazyNet60.Value;
        public static ReferenceAssemblies Net70 => LazyNet70.Value;
        public static ReferenceAssemblies Net80 => LazyNet80.Value;
    }
}
