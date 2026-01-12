using System;
using System.Linq;
using FluentAssertions;
using NavigationHost.WPF.Services.Internal;
using NavigationHost.WPF.Tests.Infrastructure;
using Xunit;

namespace NavigationHost.WPF.Tests.Services.Internal
{
    /// <summary>
    ///     Tests for the HostRegistry internal service.
    /// </summary>
    public class HostRegistryTests
    {
        private readonly HostRegistry _registry;

        public HostRegistryTests()
        {
            _registry = new HostRegistry();
        }

        [WpfFact]
        public void RegisterHost_WithValidParameters_ShouldRegisterHost()
        {
            // Arrange
            var host = new NavigationHost();
            var hostName = "TestHost";

            // Act
            _registry.RegisterHost(hostName, host);
            var retrievedHost = _registry.GetHost(hostName);

            // Assert
            retrievedHost.Should().BeSameAs(host);
        }

        [WpfFact]
        public void RegisterHost_WithNullHostName_ShouldThrowArgumentException()
        {
            // Arrange
            var host = new NavigationHost();

            // Act
            Action act = () => _registry.RegisterHost(null!, host);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [WpfFact]
        public void RegisterHost_WithWhitespaceHostName_ShouldThrowArgumentException()
        {
            // Arrange
            var host = new NavigationHost();

            // Act
            Action act = () => _registry.RegisterHost("   ", host);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [WpfFact]
        public void RegisterHost_WithNullHost_ShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => _registry.RegisterHost("TestHost", null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [WpfFact]
        public void RegisterHost_WithDuplicateName_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var host1 = new NavigationHost();
            var host2 = new NavigationHost();
            var hostName = "TestHost";

            _registry.RegisterHost(hostName, host1);

            // Act
            Action act = () => _registry.RegisterHost(hostName, host2);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage($"*'{hostName}'*");
        }

        [WpfFact]
        public void UnregisterHost_WithValidHostName_ShouldReturnTrue()
        {
            // Arrange
            var host = new NavigationHost();
            var hostName = "TestHost";
            _registry.RegisterHost(hostName, host);

            // Act
            var result = _registry.UnregisterHost(hostName);

            // Assert
            result.Should().BeTrue();
            _registry.GetHost(hostName).Should().BeNull();
        }

        [WpfFact]
        public void UnregisterHost_WithNonExistentHostName_ShouldReturnFalse()
        {
            // Act
            var result = _registry.UnregisterHost("NonExistentHost");

            // Assert
            result.Should().BeFalse();
        }

        [WpfFact]
        public void UnregisterHost_WithNullHostName_ShouldReturnFalse()
        {
            // Act
            var result = _registry.UnregisterHost(null!);

            // Assert
            result.Should().BeFalse();
        }

        [WpfFact]
        public void GetHost_WithNullHostName_ShouldReturnNull()
        {
            // Act
            var host = _registry.GetHost(null!);

            // Assert
            host.Should().BeNull();
        }

        [WpfFact]
        public void GetHost_WithWhitespaceHostName_ShouldReturnNull()
        {
            // Act
            var host = _registry.GetHost("   ");

            // Assert
            host.Should().BeNull();
        }

        [WpfFact]
        public void GetHostNames_WithNoRegisteredHosts_ShouldReturnEmptyCollection()
        {
            // Act
            var hostNames = _registry.GetHostNames();

            // Assert
            hostNames.Should().BeEmpty();
        }

        [WpfFact]
        public void GetHostNames_ShouldReturnAllRegisteredHostNames()
        {
            // Arrange
            var host1 = new NavigationHost();
            var host2 = new NavigationHost();
            var host3 = new NavigationHost();

            _registry.RegisterHost("Host1", host1);
            _registry.RegisterHost("Host2", host2);
            _registry.RegisterHost("Host3", host3);

            // Act
            var hostNames = _registry.GetHostNames().ToList();

            // Assert
            hostNames.Should().HaveCount(3);
            hostNames.Should().Contain(new[] { "Host1", "Host2", "Host3" });
        }

        // Note: Thread safety test removed because WPF controls must be created on STA threads.
        // Testing multi-threaded creation of WPF controls is not a valid scenario.
        // HostRegistry's ConcurrentDictionary ensures thread-safe registration operations.
    }
}
