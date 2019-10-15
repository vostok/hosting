using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Hosting.Helpers;

namespace Vostok.Hosting.Tests.Helpers
{
    [TestFixture]
    internal class Customization_Tests
    {
        [Test]
        public void Should_perform_customize_action_in_order()
        {
            var customization = new Customization<List<int>>();

            customization.AddCustomization(
                l => { l.Add(1); });

            customization.AddCustomization(
                l =>
                {
                    var ll = new List<int>(l) {2};
                    return ll;
                });

            customization.AddCustomization(
                l => { l.Add(3); });

            customization.AddCustomization(
                l =>
                {
                    var ll = new List<int>(l) {4};
                    return ll;
                });

            var list = customization.Customize(new List<int> {0});

            list.Should().BeEquivalentTo(new List<int> {0, 1, 2, 3, 4}, options => options.WithStrictOrdering());
        }
    }
}