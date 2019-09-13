using System;
using JetBrains.Annotations;
using Vostok.Hosting.Setup;
// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.ApplicationIdentity
{
    internal class ApplicationIdentityBuilder : IApplicationIdentityBuilder, IBuilder<VostokApplicationIdentity>
    {
        private string project;
        private string subproject;
        private string environment;
        private string application;
        private string instance;

        [NotNull]
        public VostokApplicationIdentity Build(Context context) =>
            new VostokApplicationIdentity(project, subproject, environment, application, instance);

        public IApplicationIdentityBuilder SetProject(string project)
        {
            this.project = project ?? throw new ArgumentNullException(nameof(project));
            return this;
        }

        public IApplicationIdentityBuilder SetSubproject(string subproject)
        {
            this.subproject = subproject;
            return this;
        }

        public IApplicationIdentityBuilder SetEnvironment(string environment)
        {
            this.environment = environment ?? throw new ArgumentNullException(nameof(environment));
            return this;
        }

        public IApplicationIdentityBuilder SetApplication(string application)
        {
            this.application = application ?? throw new ArgumentNullException(nameof(application));
            return this;
        }

        public IApplicationIdentityBuilder SetInstance(string instance)
        {
            this.instance = instance ?? throw new ArgumentNullException(nameof(instance));
            return this;
        }
    }
}