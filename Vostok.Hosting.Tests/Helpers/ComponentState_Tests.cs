using FluentAssertions;
using NUnit.Framework;
using Vostok.Hosting.Helpers;

namespace Vostok.Hosting.Tests.Helpers
{
    [TestFixture]
    public class ComponentState_Tests
    {
        [Test]
        public void Should_auto_enable_when_state_is_not_initialized()
        {
            var state = new ComponentState();
            state.AutoEnable();
            state.IsEnabled().Should().BeTrue();
        }
        
        [Test]
        public void Should_not_auto_enable_when_state_is_disabled()
        {
            var state = new ComponentState();
            state.Disable();
            state.AutoEnable();
            state.IsEnabled().Should().BeFalse();
        }
        
        [Test]
        public void Should_enable_when_state_is_not_initialized()
        {
            var state = new ComponentState();
            state.Enable();
            state.IsEnabled().Should().BeTrue();
        }
        
        [Test]
        public void Should_enable_when_state_is_disabled()
        {
            var state = new ComponentState();
            state.Disable();
            state.Enable();
            state.IsEnabled().Should().BeTrue();
        }
    }
}
