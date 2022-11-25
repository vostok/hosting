using Vostok.Metrics;
using Vostok.Metrics.Models;

namespace Vostok.Hosting.Components.Metrics;

internal sealed class TagsEnrichingAnnotationEventSender : IAnnotationEventSender
{
    private readonly IAnnotationEventSender baseSender;
    private readonly MetricTags tagsToAppend;

    public TagsEnrichingAnnotationEventSender(IAnnotationEventSender baseSender, MetricTags tagsToAppend)
    {
        this.baseSender = baseSender;
        this.tagsToAppend = tagsToAppend;
    }

    public void Send(AnnotationEvent @event)
    {
        var enrichedTags = @event.Tags.Append(tagsToAppend);
        var enrichedEvent = new AnnotationEvent(@event.Timestamp, enrichedTags, @event.Description);
        baseSender.Send(enrichedEvent);
    }
}