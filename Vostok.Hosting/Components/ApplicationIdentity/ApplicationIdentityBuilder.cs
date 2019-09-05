using System;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Components.ApplicationIdentity
{
    internal class ApplicationIdentityBuilder : IApplicationIdentityBuilder
    {
        private readonly VostokApplicationIdentity applicationIdentity;

        public ApplicationIdentityBuilder()
        {
            applicationIdentity = new VostokApplicationIdentity();
        }

        public VostokApplicationIdentity Build() =>
            applicationIdentity;

        public IApplicationIdentityBuilder SetProject(string project)
        {
            applicationIdentity.Project = project ?? throw new ArgumentNullException(nameof(project));
            return this;
        }

        public IApplicationIdentityBuilder SetEnvironment(string environment)
        {
            applicationIdentity.Environment = environment ?? throw new ArgumentNullException(nameof(environment));
            return this;
        }

        public IApplicationIdentityBuilder SetApplication(string application)
        {
            applicationIdentity.Application = application ?? throw new ArgumentNullException(nameof(application));
            return this;
        }

        public IApplicationIdentityBuilder SetInstance(string instance)
        {
            applicationIdentity.Instance = instance ?? throw new ArgumentNullException(nameof(instance));
            return this;
        }
    }
}