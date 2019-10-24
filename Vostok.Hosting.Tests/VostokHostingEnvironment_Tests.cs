using System;
using System.Threading;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Configuration.Abstractions;
using Vostok.Context;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;
using Vostok.Tracing.Abstractions;
// ReSharper disable AccessToModifiedClosure

namespace Vostok.Hosting.Tests
{
    [TestFixture]
    internal class VostokHostingEnvironment_Tests
    {
        [Test]
        public void ApplicationReplicationInfo_should_use_caching_transform()
        {
            var instanceIndex = 13;
            var instancesCount = 42;
            var replicationInfoProvider = new Func<(int instanceIndex, int instancesCount)>(() => (instanceIndex, instancesCount));

            var environment = new VostokHostingEnvironment(
                CancellationToken.None,
                Substitute.For<IVostokApplicationIdentity>(),
                Substitute.For<IVostokApplicationLimits>(),
                replicationInfoProvider,
                Substitute.For<IVostokApplicationMetrics>(),
                Substitute.For<ILog>(),
                Substitute.For<ITracer>(),
                Substitute.For<IHerculesSink>(),
                Substitute.For<IConfigurationSource>(),
                Substitute.For<IConfigurationProvider>(),
                Substitute.For<IClusterConfigClient>(),
                Substitute.For<IServiceBeacon>(),
                Substitute.For<IServiceLocator>(),
                Substitute.For<IContextGlobals>(),
                Substitute.For<IContextProperties>(),
                Substitute.For<IContextConfiguration>(),
                Substitute.For<ClusterClientSetup>(),
                Substitute.For<IVostokHostExtensions>(),
                () => {}
            );

            var info1 = environment.ApplicationReplicationInfo;
            info1.InstanceIndex.Should().Be(13);
            info1.InstancesCount.Should().Be(42);

            var info2 = environment.ApplicationReplicationInfo;
            info2.Should().BeSameAs(info1);

            instanceIndex = 15;
            var info3 = environment.ApplicationReplicationInfo;
            info3.InstanceIndex.Should().Be(15);
            info1.InstanceIndex.Should().Be(13);
        }
    }
}