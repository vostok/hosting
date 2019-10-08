using JetBrains.Annotations;
using Vostok.Hosting.Components.String;
using Vostok.Hosting.Setup;
// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.ApplicationIdentity
{
    internal class ApplicationIdentityBuilder : IVostokApplicationIdentityBuilder, IBuilder<VostokApplicationIdentity>
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
                // ReSharper disable once AssignNullToNotNullAttribute
                environmentBuilder?.Build(context)?.Invoke(), 
                application, 
                instance);

        public IVostokApplicationIdentityBuilder SetProject(string project)
        {
            this.project = project;
            return this;
        }

        public IVostokApplicationIdentityBuilder SetSubproject(string subproject)
        {
            this.subproject = subproject;
            return this;
        }

        public IVostokApplicationIdentityBuilder SetEnvironment(string environment)
        {
            environmentBuilder = StringProviderBuilder.FromValue(environment);
            return this;
        }

        public IVostokApplicationIdentityBuilder SetEnvironmentFromClusterConfig(string path)
        {
            environmentBuilder = StringProviderBuilder.FromClusterConfig(path, "unknown");
            return this;
        }

        public IVostokApplicationIdentityBuilder SetApplication(string application)
        {
            this.application = application;
            return this;
        }

        public IVostokApplicationIdentityBuilder SetInstance(string instance)
        {
            this.instance = instance;
            return this;
        }
    }
}