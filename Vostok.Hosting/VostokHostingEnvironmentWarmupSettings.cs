using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting;

[PublicAPI]
public class VostokHostingEnvironmentWarmupSettings
{
    /// <summary>
    /// <para>If set to <c>true</c>, logs all settings from <see cref="IVostokHostingEnvironment.ConfigurationSource"/> after assembling <see cref="IVostokHostingEnvironment"/>.</para>
    /// <para>Ignores <see cref="Vostok.Configuration.Abstractions.Attributes.SecretAttribute"/> attribute.</para>
    /// <para>Requires <see cref="WarmupConfiguration"/>.</para>
    /// </summary>
    public bool LogApplicationConfiguration { get; set; }
    
    /// <summary>
    /// <para>If set to <c>true</c>, logs all environment variables that begin with <c>DOTNET_</c>, <c>COMPlus_</c>, <c>ASPNETCORE_</c>.</para>
    /// </summary>
    public bool LogDotnetEnvironmentVariables { get; set; } = true;
    
    /// <summary>
    /// If set to <c>true</c>, warms up configuration sources before initializing the application. Required by <see cref="LogApplicationConfiguration"/>.
    /// </summary>
    public bool WarmupConfiguration { get; set; } = true;
    
    /// <summary>
    /// If set to <c>true</c>, warms up ZooKeeper client before initializing the application.
    /// </summary>
    public bool WarmupZooKeeper { get; set; } = true;
}