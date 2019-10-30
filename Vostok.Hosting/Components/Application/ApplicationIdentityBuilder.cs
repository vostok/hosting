using System;
using JetBrains.Annotations;
using Vostok.Hosting.Setup;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Application
{
    internal class ApplicationIdentityBuilder : IVostokApplicationIdentityBuilder, IBuilder<ApplicationIdentity>
    {
        private volatile string project;
        private volatile string subproject;
        private volatile string environment;
        private volatile string application;
        private volatile string instance;

        [NotNull]
        public ApplicationIdentity Build(BuildContext context) =>
            new ApplicationIdentity(
                project,
                subproject,
                environment,
                application,
                instance);

        public IVostokApplicationIdentityBuilder SetProject(string project)
        {
            if (string.IsNullOrWhiteSpace(project))
                throw new ArgumentNullException(nameof(project));
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
            if (string.IsNullOrWhiteSpace(environment))
                throw new ArgumentNullException(nameof(environment));
            this.environment = environment;
            return this;
        }

        public IVostokApplicationIdentityBuilder SetApplication(string application)
        {
            if (string.IsNullOrWhiteSpace(application))
                throw new ArgumentNullException(nameof(application));
            this.application = application;
            return this;
        }

        public IVostokApplicationIdentityBuilder SetInstance(string instance)
        {
            if (string.IsNullOrWhiteSpace(instance))
                throw new ArgumentNullException(nameof(instance));
            this.instance = instance;
            return this;
        }
    }
}