using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Tracing.Abstractions;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokClusterClientSetupTracingBuilder
    {
        IVostokClusterClientSetupTracingBuilder SetAdditionalRequestTransformation([NotNull] Func<Request, TraceContext, Request> additionalRequestTransformation);
    }
}