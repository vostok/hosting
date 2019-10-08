using System;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Tracing;
using Vostok.Hosting.Setup;
using Vostok.Tracing.Abstractions;

namespace Vostok.Hosting.Components.ClusterClient
{
    internal class ClusterClientSetupTracingBuilder : IVostokClusterClientSetupTracingBuilder, IBuilder<TracingConfiguration>
    {
        private Func<Request, TraceContext, Request> additionalRequestTransformation;

        public IVostokClusterClientSetupTracingBuilder SetAdditionalRequestTransformation(Func<Request, TraceContext, Request> additionalRequestTransformation)
        {
            this.additionalRequestTransformation = additionalRequestTransformation;
            return this;
        }

        public TracingConfiguration Build(BuildContext context)
        {
            var result = new TracingConfiguration(context.Tracer);

            if (additionalRequestTransformation != null)
                result.AdditionalRequestTransformation = additionalRequestTransformation;

            return result;
        }
    }
}