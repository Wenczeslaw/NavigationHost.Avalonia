using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NavigationHost.Abstractions;
using NavigationHost.WPF.Tests.Infrastructure;
using Xunit;

namespace NavigationHost.WPF.Tests.Services
{
    /// <summary>
    ///     Tests for the HostManager service.
    /// </summary>
    public class HostManagerTests : TestFixtureBase
    {
        [WpfFact]
        public void RegisterHost_WithValidParameters_ShouldRegisterHost()
        {
            // Arrange
            var host = new NavigationHost();
            var hostName = "TestHost";

            // Act
            HostManager.RegisterHost(hostName, host);
            var retrievedHost = HostManager.GetHost(hostName);

            // Assert
            retrievedHost.Should().BeSameAs(host);
        }

        [WpfFact]
        public void RegisterHost_WithNullHostName_ShouldThrowArgumentException()
        {
            // Arrange
            var host = new NavigationHost();

            // Act
            Action act = () => HostManager.RegisterHost(null!, host);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("hostName");
        }

        [WpfFact]
        public void RegisterHost_WithEmptyHostName_ShouldThrowArgumentException()
        {
            // Arrange
            var host = new NavigationHost();

            // Act
            Action act = () => HostManager.RegisterHost(string.Empty, host);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("hostName");
        }

        [WpfFact]
        public void RegisterHost_WithNullHost_ShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => HostManager.RegisterHost("TestHost", null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("host");
        }

        [WpfFact]
        public void RegisterHost_WithDuplicateName_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var host1 = new NavigationHost();
            var host2 = new NavigationHost();
            var hostName = "TestHost";

            HostManager.RegisterHost(hostName, host1);

            // Act
            Action act = () => HostManager.RegisterHost(hostName, host2);

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
            HostManager.RegisterHost(hostName, host);

            // Act
            var result = HostManager.UnregisterHost(hostName);

            // Assert
            result.Should().BeTrue();
            HostManager.GetHost(hostName).Should().BeNull();
        }

        [WpfFact]
        public void UnregisterHost_WithNonExistentHostName_ShouldReturnFalse()
        {
            // Act
            var result = HostManager.UnregisterHost("NonExistentHost");

            // Assert
            result.Should().BeFalse();
        }

        [WpfFact]
        public void GetHost_WithNonExistentHostName_ShouldReturnNull()
        {
            // Act
            var host = HostManager.GetHost("NonExistentHost");

            // Assert
            host.Should().BeNull();
        }

        [WpfFact]
        public void GetHostNames_ShouldReturnAllRegisteredHostNames()
        {
            // Arrange
            var host1 = new NavigationHost();
            var host2 = new NavigationHost();
            
            HostManager.RegisterHost("Host1", host1);
            HostManager.RegisterHost("Host2", host2);

            // Act
            var hostNames = HostManager.GetHostNames().ToList();

            // Assert
            hostNames.Should().HaveCount(2);
            hostNames.Should().Contain("Host1");
            hostNames.Should().Contain("Host2");
        }

        [WpfFact]
        public void Navigate_WithContent_ShouldNavigateToSpecifiedHost()
        {
            // Arrange
            var host = new NavigationHost();
            var hostName = "TestHost";
            var view = new TestView();
            
            HostManager.RegisterHost(hostName, host);

            // Act
            HostManager.Navigate(hostName, view);

            // Assert
            host.CurrentContent.Should().BeSameAs(view);
        }

        [WpfFact]
        public void Navigate_WithNullHostName_ShouldThrowArgumentException()
        {
            // Arrange
            var view = new TestView();

            // Act
            Action act = () => HostManager.Navigate(null!, view);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("hostName");
        }

        [WpfFact]
        public void Navigate_WithNullContent_ShouldThrowArgumentNullException()
        {
            // Arrange
            var host = new NavigationHost();
            HostManager.RegisterHost("TestHost", host);

            // Act
            Action act = () => HostManager.Navigate("TestHost", (object)null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("content");
        }

        [WpfFact]
        public void Navigate_WithNonExistentHost_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var view = new TestView();

            // Act
            Action act = () => HostManager.Navigate("NonExistentHost", view);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*NonExistentHost*");
        }

        [WpfFact]
        public void Navigate_WithType_ShouldCreateAndNavigateToView()
        {
            // Arrange
            var host = new NavigationHost();
            var hostName = "TestHost";
            
            HostManager.RegisterHost(hostName, host);

            // Act
            HostManager.Navigate(hostName, typeof(TestView));

            // Assert
            host.CurrentContent.Should().NotBeNull();
            host.CurrentContent.Should().BeOfType<TestView>();
        }

        [WpfFact]
        public void Navigate_Generic_ShouldCreateAndNavigateToView()
        {
            // Arrange
            var host = new NavigationHost();
            var hostName = "TestHost";
            
            HostManager.RegisterHost(hostName, host);

            // Act
            HostManager.Navigate<TestView>(hostName);

            // Assert
            host.CurrentContent.Should().NotBeNull();
            host.CurrentContent.Should().BeOfType<TestView>();
        }

        [WpfFact]
        public void Navigate_WithParameter_ShouldPassParameterToViewModel()
        {
            // Arrange
            var services = new ServiceCollection();
            ConfigureServices(services);
            services.AddTransient<TestView>();
            services.AddTransient<TestViewModel>();
            
            var provider = services.BuildServiceProvider();
            var manager = provider.GetRequiredService<IHostManager>();
            
            var host = new NavigationHost();
            manager.RegisterHost("TestHost", host);
            
            var parameter = new { Id = 123 };

            // Act
            manager.Navigate<TestView>("TestHost", parameter);

            // Assert
            host.CurrentContent.Should().NotBeNull();
            host.CurrentContent.Should().BeOfType<TestView>();
        }
    }
}

