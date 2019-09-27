using JetBrains.Annotations;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Hosting.Components.String;
using Vostok.Hosting.Setup;
// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.ApplicationIdentity
{
    internal class ApplicationIdentityBuilder : IApplicationIdentityBuilder, IBuilder<VostokApplicationIdentity>
    {
        private string project;
        private string subproject;
        private StringProviderBuilder environmentBuilder;
        private string application;
        private string instance;
        
        [NotNull]
        public VostokApplicationIdentity Build(BuildContext context) =>
            new VostokApplicationIdentity(
                project,
                subproject,
                environmentBuilder?.Build(context)?.Invoke() ?? "unknown", 
                application, 
                instance);

        public IApplicationIdentityBuilder SetProject(string project)
        {
            this.project = project;
            return this;
        }

        public IApplicationIdentityBuilder SetSubproject(string subproject)
        {
            this.subproject = subproject;
            return this;
        }

        public IApplicationIdentityBuilder SetEnvironment(string environment)
        {
            environmentBuilder = StringProviderBuilder.FromValue(environment);
            return this;
        }

        public IApplicationIdentityBuilder SetEnvironmentFromClusterConfig(string path)
        {
            environmentBuilder = StringProviderBuilder.FromClusterConfig(path);
            return this;
        }

        public IApplicationIdentityBuilder SetApplication(string application)
        {
            this.application = application;
            return this;
        }

        public IApplicationIdentityBuilder SetInstance(string instance)
        {
            this.instance = instance;
            return this;
        }
    }
}