using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions.Diagnostics;
using Vostok.Hosting.Components.Diagnostics;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Tests.Components.Diagnostics;

[TestFixture]
internal class HealthTracker_Tests
{
    private List<CustomCheck> capturedEvents;
    private HealthTracker healthTracker;

    [SetUp]
    public void SetUp()
    {
        capturedEvents = new List<CustomCheck>();
        healthTracker = new HealthTracker(100.Milliseconds(), new SilentLog());
    }

    [TearDown]
    public void TearDown()
    {
        healthTracker.Dispose();
    }

    [Test]
    public async Task HealthTracker_ShouldStartPeriodicChecks_OnlyAfterTaskWasTriggered()
    {
        healthTracker.RegisterCheck("custom", new CustomCheck(capturedEvents));

        capturedEvents.Should().BeEmpty();

        var tcs = new TaskCompletionSource<bool>();

        healthTracker.PrepareToLaunchPeriodicalChecks(tcs.Task, CancellationToken.None);

        await Task.Delay(200.Milliseconds());

        capturedEvents.Should().BeEmpty();

        tcs.SetResult(true);

        await Task.Delay(200.Milliseconds());

        capturedEvents.Should().NotBeEmpty();
    }

    [Test]
    public async Task HealthTracker_ShouldStartPeriodicChecks_IfTaskIsNull()
    {
        healthTracker.RegisterCheck("custom", new CustomCheck(capturedEvents));

        capturedEvents.Should().BeEmpty();

        healthTracker.PrepareToLaunchPeriodicalChecks(null, CancellationToken.None);

        await Task.Delay(200.Milliseconds());

        capturedEvents.Should().NotBeEmpty();
    }

    private class CustomCheck : IHealthCheck
    {
        private readonly List<CustomCheck> capturedEvents;

        public CustomCheck(List<CustomCheck> capturedEvents)
        {
            this.capturedEvents = capturedEvents;
        }

        public Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken)
        {
            capturedEvents.Add(this);
            return Task.FromResult(new HealthCheckResult(HealthStatus.Healthy, null));
        }
    }
}