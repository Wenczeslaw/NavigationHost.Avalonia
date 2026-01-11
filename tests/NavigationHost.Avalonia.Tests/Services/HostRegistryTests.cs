using FluentAssertions;
using NavigationHost.Avalonia.Services.Internal;

namespace NavigationHost.Avalonia.Tests.Services
{
    public class HostRegistryTests
    {
        private readonly HostRegistry _sut;

        public HostRegistryTests()
        {
            _sut = new HostRegistry();
        }

        [Fact]
        public void RegisterHost_WithValidParameters_ShouldRegisterHost()
        {
            // Arrange
            var hostName = "TestHost";
            var host = new NavigationHost();

            // Act
            _sut.RegisterHost(hostName, host);

            // Assert
            var retrievedHost = _sut.GetHost(hostName);
            retrievedHost.Should().BeSameAs(host);
        }

        [Fact]
        public void RegisterHost_WithNullHostName_ShouldThrowArgumentException()
        {
            // Arrange
            var host = new NavigationHost();

            // Act
            Action act = () => _sut.RegisterHost(null!, host);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Host name cannot be null or whitespace*");
        }

        [Fact]
        public void RegisterHost_WithEmptyHostName_ShouldThrowArgumentException()
        {
            // Arrange
            var host = new NavigationHost();

            // Act
            Action act = () => _sut.RegisterHost(string.Empty, host);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Host name cannot be null or whitespace*");
        }

        [Fact]
        public void RegisterHost_WithNullHost_ShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => _sut.RegisterHost("TestHost", null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("host");
        }

        [Fact]
        public void RegisterHost_WithDuplicateHostName_ShouldReplaceExistingHost()
        {
            // Arrange
            var hostName = "TestHost";
            var firstHost = new NavigationHost();
            var secondHost = new NavigationHost();

            // Act
            _sut.RegisterHost(hostName, firstHost);
            _sut.RegisterHost(hostName, secondHost);

            // Assert
            var retrievedHost = _sut.GetHost(hostName);
            retrievedHost.Should().BeSameAs(secondHost);
            retrievedHost.Should().NotBeSameAs(firstHost);
        }

        [Fact]
        public void UnregisterHost_WithExistingHost_ShouldReturnTrueAndRemoveHost()
        {
            // Arrange
            var hostName = "TestHost";
            var host = new NavigationHost();
            _sut.RegisterHost(hostName, host);

            // Act
            var result = _sut.UnregisterHost(hostName);

            // Assert
            result.Should().BeTrue();
            _sut.GetHost(hostName).Should().BeNull();
        }

        [Fact]
        public void UnregisterHost_WithNonExistingHost_ShouldReturnFalse()
        {
            // Act
            var result = _sut.UnregisterHost("NonExistingHost");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void UnregisterHost_WithNullHostName_ShouldReturnFalse()
        {
            // Act
            var result = _sut.UnregisterHost(null!);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GetHost_WithExistingHost_ShouldReturnHost()
        {
            // Arrange
            var hostName = "TestHost";
            var host = new NavigationHost();
            _sut.RegisterHost(hostName, host);

            // Act
            var retrievedHost = _sut.GetHost(hostName);

            // Assert
            retrievedHost.Should().BeSameAs(host);
        }

        [Fact]
        public void GetHost_WithNonExistingHost_ShouldReturnNull()
        {
            // Act
            var result = _sut.GetHost("NonExistingHost");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetHost_WithNullHostName_ShouldReturnNull()
        {
            // Act
            var result = _sut.GetHost(null!);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetHostNames_WithNoRegisteredHosts_ShouldReturnEmptyCollection()
        {
            // Act
            var hostNames = _sut.GetHostNames();

            // Assert
            hostNames.Should().BeEmpty();
        }

        [Fact]
        public void GetHostNames_WithRegisteredHosts_ShouldReturnAllHostNames()
        {
            // Arrange
            _sut.RegisterHost("Host1", new NavigationHost());
            _sut.RegisterHost("Host2", new NavigationHost());
            _sut.RegisterHost("Host3", new NavigationHost());

            // Act
            var hostNames = _sut.GetHostNames();

            // Assert
            hostNames.Should().HaveCount(3);
            hostNames.Should().Contain(new[] { "Host1", "Host2", "Host3" });
        }

        [Fact]
        public void GetHostNames_AfterUnregisteringHost_ShouldNotIncludeUnregisteredHost()
        {
            // Arrange
            _sut.RegisterHost("Host1", new NavigationHost());
            _sut.RegisterHost("Host2", new NavigationHost());
            _sut.UnregisterHost("Host1");

            // Act
            var hostNames = _sut.GetHostNames();

            // Assert
            hostNames.Should().HaveCount(1);
            hostNames.Should().Contain("Host2");
            hostNames.Should().NotContain("Host1");
        }
    }
}

