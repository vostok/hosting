using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Models;
using Vostok.Hosting.Setup;
using Vostok.Metrics;
using Vostok.Metrics.Models;

namespace Vostok.Hosting.Tests;

[TestFixture]
internal sealed class VostokHost_Tests_Annotations
{
    private IAnnotationEventSender annotationEventSender;
    private List<AnnotationEvent> receivedEvents;

    [SetUp]
    public void SetUp()
    {
        annotationEventSender = Substitute.For<IAnnotationEventSender, IMetricEventSender>();
        annotationEventSender
            .When(sender => sender.Send(Arg.Any<AnnotationEvent>()))
            .Do(info => receivedEvents.Add(info.Arg<AnnotationEvent>()));

        receivedEvents = new List<AnnotationEvent>();
    }
    
    [Test]
    public void Should_not_send_lifecycle_annotations_when_not_allowed()
    {
        var app = new Application();

        var host = new VostokHost(new TestHostSettings(app, SetupEnvironment)
        {
            SendAnnotations = false
        });

        host.Start(VostokApplicationState.Running);
        host.Stop();

        receivedEvents.Should().BeEmpty();
    }
    
    [Test]
    public void Should_send_lifecycle_annotations_when_allowed()
    {
        var app = new Application();

        var host = new VostokHost(new TestHostSettings(app, SetupEnvironment)
        {
            SendAnnotations = true
        });

        host.Start(VostokApplicationState.Running);
        host.Stop();

        receivedEvents.Should().NotBeEmpty();
    }
    
    [Test]
    public void Should_enrich_all_annotations_with_identity_tags()
    {
        var app = new Application();

        var host = new VostokHost(new TestHostSettings(app, SetupEnvironment)
        {
            SendAnnotations = true
        });

        host.Start(VostokApplicationState.Running);
        host.Stop();

        receivedEvents.Should().NotBeEmpty();
        
        CheckEventsContainTags(WellKnownApplicationIdentityProperties.Project, "project");
        CheckEventsContainTags(WellKnownApplicationIdentityProperties.Subproject, "subproject");
        CheckEventsContainTags(WellKnownApplicationIdentityProperties.Application, "application");
        CheckEventsContainTags(WellKnownApplicationIdentityProperties.Environment, "environment");
        CheckEventsContainTags(WellKnownApplicationIdentityProperties.Instance, "instance");
    }

    [Test]
    public void Should_enrich_all_annotations_with_provided_tags_when_has_instance_tag()
    {
        var app = new Application(writeInstanceAnnotation: true);

        var (key, value) = ("key", "value");
        var host = new VostokHost(new TestHostSettings(app,
            builder =>
            {
                SetupEnvironment(builder);
                builder.SetupMetrics(m => m.EnrichInstanceAnnotationTags((key, value)));
            })
        {
            SendAnnotations = true
        });

        host.Start(VostokApplicationState.Running);
        host.Stop();

        receivedEvents.Should().NotBeEmpty();

        CheckEventsContainTags(key, value);
    }
    
    [Test]
    public void Should_not_enrich_annotations_with_provided_tags_when_has_not_instance_tag()
    {
        var app = new Application(writeNonInstanceAnnotation: true);

        var (key, value) = ("key", "value");
        var host = new VostokHost(new TestHostSettings(app,
            builder =>
            {
                SetupEnvironment(builder);
                builder.SetupMetrics(m => m.EnrichInstanceAnnotationTags((key, value)));
            }));

        host.Start(VostokApplicationState.Running);
        host.Stop();

        receivedEvents.Should().NotBeEmpty();
        var nonInstanceAnnotation = receivedEvents.Single(a => a.Description == Application.NonInstanceAnnotationDescription);
        nonInstanceAnnotation.Tags.Contains(new MetricTag(key, value)).Should().BeFalse();
    }

    [Test]
    public void Should_not_enrich_metrics_with_provided_annotations_tags()
    {
        var app = new Application(writeApplicationMetric: true);

        var receivedMetrics = new List<MetricEvent>();

        var metricSender = Substitute.For<IMetricEventSender>();
        metricSender.When(s => s.Send(Arg.Any<MetricEvent>())).Do(info => receivedMetrics.Add(info.Arg<MetricEvent>()));

        var (key, value) = ("key", "value");
        var host = new VostokHost(new TestHostSettings(app,
            builder =>
            {
                SetupEnvironment(builder);
                builder.SetupMetrics(m => m
                    .AddMetricEventSender(metricSender)
                    .EnrichInstanceAnnotationTags((key, value)));
            }));

        host.Start(VostokApplicationState.Running);
        host.Stop();

        receivedMetrics.Should().NotBeEmpty();
        receivedMetrics.All(m => !m.Tags.Contains(new MetricTag(key, value))).Should().BeTrue();
    }

    private void SetupEnvironment(IVostokHostingEnvironmentBuilder builder)
    {
        builder.SetupApplicationIdentity(id => id
            .SetProject("project")
            .SetSubproject("subproject")
            .SetApplication("application")
            .SetEnvironment("environment")
            .SetInstance("instance"));
        
        builder.SetupLog(log => log.SetupConsoleLog());
        builder.SetupShutdownTimeout(1.Seconds());

        builder.SetupMetrics(m => m.AddMetricEventSender((IMetricEventSender)annotationEventSender));
    }

    private void CheckEventsContainTags(string tagKey, string tagValue) =>
        receivedEvents
            .All(@event => @event.Tags.Contains(new MetricTag(tagKey, tagValue)))
            .Should()
            .BeTrue();

    private class Application : IVostokApplication
    {
        public const string NonInstanceAnnotationDescription = "NonInstanceAnnotation";
        
        private readonly bool writeInstanceAnnotation;
        private readonly bool writeNonInstanceAnnotation;
        private readonly bool writeApplicationMetric;

        public Application(bool writeInstanceAnnotation = false, bool writeNonInstanceAnnotation = false, bool writeApplicationMetric = false)
        {
            this.writeInstanceAnnotation = writeInstanceAnnotation;
            this.writeNonInstanceAnnotation = writeNonInstanceAnnotation;
            this.writeApplicationMetric = writeApplicationMetric;
        }

        public Task InitializeAsync(IVostokHostingEnvironment environment) =>
            Task.CompletedTask;

        public Task RunAsync(IVostokHostingEnvironment environment)
        {
            if (writeInstanceAnnotation)
                environment.Metrics.Instance.SendAnnotation("Annotation");
            
            if (writeNonInstanceAnnotation)
                environment.Metrics.Project.SendAnnotation(NonInstanceAnnotationDescription);

            if (writeApplicationMetric)
                environment.Metrics.Instance.Send(new MetricDataPoint(42, ("metricKey", "metricValue")));

            return Task.Delay(-1, environment.ShutdownToken);
        }
    }
}