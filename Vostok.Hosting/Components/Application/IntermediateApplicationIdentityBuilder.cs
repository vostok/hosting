using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.Components.Application
{
    internal class IntermediateApplicationIdentityBuilder : ApplicationIdentityBuilder
    {
        public override IVostokApplicationIdentity Build(BuildContext context) =>
            new IntermediateApplicationIdentity(project, subproject, environment, application, instance);
    }
}