using System.Linq;
using Vostok.Hosting.Abstractions;
using Vostok.Metrics;
using Vostok.Metrics.Models;

namespace Vostok.Hosting.Components.Metrics;

internal sealed class TagsEnrichingInstanceAnnotationEventSender : IAnnotationEventSender
{
    private readonly IAnnotationEventSender baseSender;
    private readonly MetricTags tagsToAppend;

    public TagsEnrichingInstanceAnnotationEventSender(IAnnotationEventSender baseSender, MetricTags tagsToAppend)
    {
        this.baseSender = baseSender;
        this.tagsToAppend = tagsToAppend;
    }

    public void Send(AnnotationEvent @event)
    {
        if (@event.Tags.Any(metricTag => metricTag.Key == WellKnownApplicationIdentityProperties.Instance))
        {
            var enrichedTags = @event.Tags.Append(tagsToAppend);
            @event = new AnnotationEvent(@event.Timestamp, enrichedTags, @event.Description);
        }

        baseSender.Send(@event);
    }
}