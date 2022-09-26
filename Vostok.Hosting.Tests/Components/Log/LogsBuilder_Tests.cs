using FluentAssertions;
using NUnit.Framework;
using Vostok.Hosting.Components.Log;

namespace Vostok.Hosting.Tests.Components.Log
{
    [TestFixture]
    internal class LogsBuilder_Tests
    {
        [Test]
        public void Should_auto_enable_console_log_when_setup()
        {
            var builder = new LogsBuilder()
                .SetupConsoleLog();

            builder.IsConsoleLogEnabled.Should().BeTrue();
        }

        [Test]
        public void Should_not_auto_enable_console_log_when_manually_disabled()
        {
            var builder = new LogsBuilder()
                .SetupConsoleLog(b => b.Disable())
                .SetupConsoleLog(_ => {});

            builder.IsConsoleLogEnabled.Should().BeFalse();
        }

        [Test]
        public void Should_auto_enable_file_log_when_setup()
        {
            var builder = new LogsBuilder()
                .SetupFileLog(_ => {});

            builder.IsFileLogEnabled.Should().BeTrue();
        }

        [Test]
        public void Should_not_auto_enable_file_log_when_manually_disabled()
        {
            var builder = new LogsBuilder()
                .SetupFileLog(b => b.Disable())
                .SetupFileLog(_ => {});

            builder.IsFileLogEnabled.Should().BeFalse();
        }

        [Test]
        public void Should_auto_enable_hercules_log_when_setup()
        {
            var builder = new LogsBuilder()
                .SetupHerculesLog(_ => {});

            builder.IsHerculesLogEnabled.Should().BeTrue();
        }

        [Test]
        public void Should_not_auto_enable_hercules_log_when_manually_disabled()
        {
            var builder = new LogsBuilder()
                .SetupHerculesLog(b => b.Disable())
                .SetupHerculesLog(_ => {});

            builder.IsHerculesLogEnabled.Should().BeFalse();
        }
    }
}
