using FluentAssertions;
using NUnit.Framework;
using Vostok.Hosting.Components.Log;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Tests.Components.Log
{
    [TestFixture]
    internal class LogsBuilder_Tests
    {
        [Test]
        public void Should_auto_enable_console_log_when_setup()
        {
            var builder = new LogsBuilder()
                .SetupConsoleLog(_ => {});

            builder.IsConsoleLogEnabled.Should().BeTrue();
        }

        [Test]
        public void Should_disable_console_log_after_auto_enable()
        {
            var builder = new LogsBuilder()
                .SetupConsoleLog(_ => {})
                .SetupConsoleLog(b => b.Disable());

            builder.IsConsoleLogEnabled.Should().BeFalse();
        }

        [Test]
        public void Should_disable_console_log_after_manually_enable()
        {
            var builder = new LogsBuilder()
                .SetupConsoleLog(b => b.Enable())
                .SetupConsoleLog(b => b.Disable());

            builder.IsConsoleLogEnabled.Should().BeFalse();
        }

        [Test]
        public void Should_not_auto_enable_console_log_when_manually_disabled()
        {
            var builder = new LogsBuilder()
                .SetupConsoleLog(_ => {})
                .SetupConsoleLog(b => b.Disable())
                .SetupConsoleLog(_ => {});

            builder.IsConsoleLogEnabled.Should().BeFalse();
        }

        [Test]
        public void Should_enable_console_log_when_manually_disabled_and_then_manually_enabled()
        {
            var builder = new LogsBuilder()
                .SetupConsoleLog(_ => {})
                .SetupConsoleLog(b => b.Disable())
                .SetupConsoleLog(b => b.Enable());

            builder.IsConsoleLogEnabled.Should().BeTrue();
        }

        [Test]
        public void Should_enable_console_log_when_manually_disabled_and_then_enabled_with_extension()
        {
            var builder = new LogsBuilder()
                .SetupConsoleLog(_ => {})
                .SetupConsoleLog(b => b.Disable())
                .SetupConsoleLog();

            builder.IsConsoleLogEnabled.Should().BeTrue();
        }
    }
}