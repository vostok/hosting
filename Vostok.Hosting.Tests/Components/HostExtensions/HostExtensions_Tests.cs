using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace Vostok.Hosting.Tests.Components.HostExtensions
{
    [TestFixture]
    internal class HostExtensions_Tests
    {
        [Test]
        public void Should_return_registered_items()
        {
            var extensions = new Hosting.Components.HostExtensions.HostExtensions();

            extensions.Add(42);
            extensions.Add("hello");

            extensions.Add("a", 1);
            extensions.Add("b", 2);

            extensions.Get<int>().Should().Be(42);
            extensions.Get<string>().Should().Be("hello");

            extensions.Get<int>("a").Should().Be(1);
            extensions.Get<int>("b").Should().Be(2);

            extensions.GetAll().Should().BeEquivalentTo((typeof(int), 42), (typeof(string), "hello"));
        }

        [Test]
        public void Should_throw_key_not_found()
        {
            var extensions = new Hosting.Components.HostExtensions.HostExtensions();

            extensions.Add(42);
            extensions.Add("hello");

            extensions.Add("a", 1);
            extensions.Add("b", 2);

            ((Action)(() => extensions.Get<char>()))
                .Should()
                .Throw<KeyNotFoundException>();

            ((Action)(() => extensions.Get<int>("c")))
                .Should()
                .Throw<KeyNotFoundException>();
        }
    }
}