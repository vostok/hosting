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
        if (ContainsTag(@event.Tags, WellKnownApplicationIdentityProperties.Instance))
        {
            var enrichedTags = @event.Tags.Append(tagsToAppend);
            @event = new AnnotationEvent(@event.Timestamp, enrichedTags, @event.Description);
        }

        baseSender.Send(@event);
    }

    private static bool ContainsTag(MetricTags tags, string tag)
    {
        foreach (var metricTag in tags)
        {
            if (metricTag.Key == tag)
                return true;
        }

        return false;
    }
}