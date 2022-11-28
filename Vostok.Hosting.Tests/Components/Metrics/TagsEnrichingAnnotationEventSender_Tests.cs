using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Hosting.Components.Metrics;
using Vostok.Metrics;
using Vostok.Metrics.Models;

namespace Vostok.Hosting.Tests.Components.Metrics;

internal sealed class TagsEnrichingAnnotationEventSender_Tests
{
    [Test]
    public void Should_enrich_annotation_tags()
    {
        var baseSender = Substitute.For<IAnnotationEventSender>();

        AnnotationEvent enrichedEvent = null;
        baseSender.When(s => s.Send(Arg.Any<AnnotationEvent>())).Do(info => enrichedEvent = info.Arg<AnnotationEvent>());

        var initialTag = new MetricTag("initialTagKey", "initialTagValue");
        var newTag = new MetricTag("key", "value");
        
        var sender = new TagsEnrichingInstanceAnnotationEventSender(baseSender, new MetricTags(newTag));
        sender.Send(new AnnotationEvent(DateTimeOffset.Now, new MetricTags(initialTag), "Annotation"));

        enrichedEvent.Tags.Should().BeEquivalentTo(initialTag, newTag);
    }
}