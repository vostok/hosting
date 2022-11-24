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
internal sealed class VostokHost_Tests_LifecycleAnnotations
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
    public void Should_not_send_annotations_when_not_allowed()
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
    public void Should_send_annotations_when_allowed()
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
    public void Should_enrich_annotations_with_provided_tags()
    {
        var app = new Application();

        var (key, value) = ("key", "value");
        var host = new VostokHost(new TestHostSettings(app,
            builder =>
            {
                SetupEnvironment(builder);
                builder.EnrichLifecycleAnnotationTags((key, value));
            }));

        host.Start(VostokApplicationState.Running);
        host.Stop();

        receivedEvents.Should().NotBeEmpty();
        receivedEvents.All(@event => @event.Tags.Contains(new MetricTag(key, value))).Should().BeTrue();
    }

    private void SetupEnvironment(IVostokHostingEnvironmentBuilder builder)
    {
        builder.SetupApplicationIdentity(id => id.SetProject("test").SetApplication("test").SetEnvironment("test").SetInstance("test"));
        builder.SetupLog(log => log.SetupConsoleLog());
        builder.SetupShutdownTimeout(1.Seconds());

        builder.SetupMetrics(m => m.AddMetricEventSender((IMetricEventSender)annotationEventSender));
    }

    private class Application : IVostokApplication
    {
        public Task InitializeAsync(IVostokHostingEnvironment environment) =>
            Task.CompletedTask;

        public Task RunAsync(IVostokHostingEnvironment environment)
        {
            var source = new TaskCompletionSource<bool>(false);

            environment.ShutdownToken.Register(() => source.TrySetResult(true));

            return source.Task;
        }
    }
}