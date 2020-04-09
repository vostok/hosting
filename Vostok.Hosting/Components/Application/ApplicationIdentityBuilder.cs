using System;
using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Application
{
    internal class ApplicationIdentityBuilder : IVostokApplicationIdentityBuilder, IBuilder<IVostokApplicationIdentity>
    {
        protected volatile string project = System.Environment.GetEnvironmentVariable(EnvironmentVariables.IdentityProject);
        protected volatile string subproject = System.Environment.GetEnvironmentVariable(EnvironmentVariables.IdentitySubproject);
        protected volatile string environment = System.Environment.GetEnvironmentVariable(EnvironmentVariables.IdentityEnvironment);
        protected volatile string application = System.Environment.GetEnvironmentVariable(EnvironmentVariables.IdentityApplication);
        protected volatile string instance = System.Environment.GetEnvironmentVariable(EnvironmentVariables.IdentityInstance);

        [NotNull]
        public virtual IVostokApplicationIdentity Build(BuildContext context) =>
            new ApplicationIdentity(project, subproject, environment, application, instance);

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