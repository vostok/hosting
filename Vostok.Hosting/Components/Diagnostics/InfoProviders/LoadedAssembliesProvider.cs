using System;
using System.Linq;
using Vostok.Commons.Environment;
using Vostok.Hosting.Abstractions.Diagnostics;

namespace Vostok.Hosting.Components.Diagnostics.InfoProviders
{
    internal class LoadedAssembliesProvider : IDiagnosticInfoProvider
    {
        public object Query() =>
            AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(assembly => !assembly.IsDynamic)
                .Select(assembly => new
                {
                    assembly.GetName().Name,
                    assembly.GetName().Version,
                    assembly.Location,
                    CommitHash = AssemblyCommitHashExtractor.ExtractFromAssembly(assembly),
                    BuildTimestamp = AssemblyBuildTimeExtractor.ExtractFromAssembly(assembly)?.ToString("u")
                })
                .ToArray();
    }
}
