using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components.Metrics;
using Vostok.Metrics;
using Vostok.Metrics.Models;

namespace Vostok.Hosting.Tests.Components.Metrics;

internal sealed class TagsEnrichingAnnotationEventSender_Tests
{
    private IAnnotationEventSender baseSender;
    private AnnotationEvent enrichedEvent;

    [SetUp]
    public void SetUp()
    {
        baseSender = Substitute.For<IAnnotationEventSender>();
        baseSender.When(s => s.Send(Arg.Any<AnnotationEvent>())).Do(info => enrichedEvent = info.Arg<AnnotationEvent>());
    }
    
    [Test]
    public void Should_enrich_annotation_tags_when_has_instance_tag()
    {
        var instanceTag = new MetricTag(WellKnownApplicationIdentityProperties.Instance, "instance");
        var additionalTag = new MetricTag("initialTagKey", "initialTagValue");

        var newTag = new MetricTag("key", "value");
        var sender = new TagsEnrichingInstanceAnnotationEventSender(baseSender, new MetricTags(newTag));

        sender.Send(new AnnotationEvent(DateTimeOffset.Now, new MetricTags(instanceTag, additionalTag), "Annotation"));

        enrichedEvent.Tags.Should().BeEquivalentTo(instanceTag, additionalTag, newTag);
    }
    
    [Test]
    public void Should_not_enrich_annotation_tags_when_has_not_instance_tag()
    {
        var additionalTag = new MetricTag("initialTagKey", "initialTagValue");
        var newTag = new MetricTag("key", "value");

        var sender = new TagsEnrichingInstanceAnnotationEventSender(baseSender, new MetricTags(newTag));
        sender.Send(new AnnotationEvent(DateTimeOffset.Now, new MetricTags(additionalTag), "Annotation"));

        enrichedEvent.Tags.Should().BeEquivalentTo(additionalTag);
    }
}