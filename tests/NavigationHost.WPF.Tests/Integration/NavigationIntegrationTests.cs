using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NavigationHost.WPF.Abstractions;
using NavigationHost.WPF.Extensions;
using NavigationHost.WPF.Tests.Infrastructure;
using Xunit;

namespace NavigationHost.WPF.Tests.Integration
{
    /// <summary>
    ///     Integration tests for end-to-end navigation scenarios.
    /// </summary>
    public class NavigationIntegrationTests : TestFixtureBase
    {
        [WpfFact]
        public void EndToEnd_RegisterAndNavigate_ShouldWorkCorrectly()
        {
            // Arrange
            var host = new NavigationHost();
            var hostName = "MainHost";
            
            HostManager.RegisterHost(hostName, host);

            // Act
            HostManager.Navigate<TestView>(hostName);

            // Assert
            host.CurrentContent.Should().NotBeNull();
            host.CurrentContent.Should().BeOfType<TestView>();
        }

        [WpfFact]
        public void EndToEnd_MultipleHosts_ShouldNavigateIndependently()
        {
            // Arrange
            var host1 = new NavigationHost();
            var host2 = new NavigationHost();
            
            HostManager.RegisterHost("Host1", host1);
            HostManager.RegisterHost("Host2", host2);

            // Act
            HostManager.Navigate<TestView>("Host1");
            HostManager.Navigate<AnotherTestView>("Host2");

            // Assert
            host1.CurrentContent.Should().BeOfType<TestView>();
            host2.CurrentContent.Should().BeOfType<AnotherTestView>();
        }

        [WpfFact]
        public void EndToEnd_NavigationWithParameter_ShouldPassToViewModel()
        {
            // Arrange
            var services = new ServiceCollection();
            ConfigureServices(services);
            services.AddView<TestView, TestViewModel>();
            
            var provider = services.BuildServiceProvider();
            var manager = provider.GetRequiredService<IHostManager>();
            
            var host = new NavigationHost();
            manager.RegisterHost("TestHost", host);
            
            var parameter = new { Id = 999, Name = "Test" };

            // Act
            manager.Navigate<TestView>("TestHost", parameter);

            // Assert
            host.CurrentContent.Should().NotBeNull();
            
            // The view should have been created
            var view = host.CurrentContent as TestView;
            view.Should().NotBeNull();
        }

        [WpfFact]
        public void EndToEnd_NavigationEvents_ShouldFireInCorrectOrder()
        {
            // Arrange
            var host = new NavigationHost();
            var hostName = "TestHost";
            
            HostManager.RegisterHost(hostName, host);

            var eventsRaised = new System.Collections.Generic.List<string>();
            
            host.Navigated += (s, e) => eventsRaised.Add("Navigated1");

            // Act
            HostManager.Navigate<TestView>(hostName);
            
            host.Navigated += (s, e) => eventsRaised.Add("Navigated2");
            
            HostManager.Navigate<AnotherTestView>(hostName);

            // Assert - First navigation fires first handler, second navigation fires both handlers
            eventsRaised.Should().HaveCount(3);
            eventsRaised[0].Should().Be("Navigated1");
            eventsRaised[1].Should().Be("Navigated1");
            eventsRaised[2].Should().Be("Navigated2");
        }

        [WpfFact]
        public void EndToEnd_UnregisterHost_ShouldPreventNavigation()
        {
            // Arrange
            var host = new NavigationHost();
            var hostName = "TestHost";
            
            HostManager.RegisterHost(hostName, host);
            HostManager.Navigate<TestView>(hostName);
            
            // Act
            var unregistered = HostManager.UnregisterHost(hostName);

            // Assert
            unregistered.Should().BeTrue();
            
            Action act = () => HostManager.Navigate<AnotherTestView>(hostName);
            act.Should().Throw<InvalidOperationException>();
        }

        [WpfFact]
        public void EndToEnd_ConventionBasedResolution_ShouldResolveViewModel()
        {
            // Arrange
            var services = new ServiceCollection();
            ConfigureServices(services);
            services.AddView<TestView>();
            services.AddTransient<TestViewModel>();
            
            var provider = services.BuildServiceProvider();
            var manager = provider.GetRequiredService<IHostManager>();
            
            var host = new NavigationHost();
            manager.RegisterHost("TestHost", host);

            // Act
            manager.Navigate<TestView>("TestHost");

            // Assert
            host.CurrentContent.Should().NotBeNull();
            host.CurrentContent.Should().BeOfType<TestView>();
        }

        [WpfFact]
        public void EndToEnd_MultipleNavigations_ShouldUpdateCorrectly()
        {
            // Arrange
            var host = new NavigationHost();
            HostManager.RegisterHost("TestHost", host);

            // Act & Assert
            HostManager.Navigate<TestView>("TestHost");
            host.CurrentContent.Should().BeOfType<TestView>();

            HostManager.Navigate<AnotherTestView>("TestHost");
            host.CurrentContent.Should().BeOfType<AnotherTestView>();

            HostManager.Navigate<TestView>("TestHost");
            host.CurrentContent.Should().BeOfType<TestView>();
        }

        [WpfFact]
        public void EndToEnd_WithDependencyInjection_ShouldInjectDependencies()
        {
            // Arrange
            var services = new ServiceCollection();
            ConfigureServices(services);
            services.AddTransient<TestViewModel>();
            services.AddTransient<ViewWithDependency>();
            
            var provider = services.BuildServiceProvider();
            var manager = provider.GetRequiredService<IHostManager>();
            
            var host = new NavigationHost();
            manager.RegisterHost("TestHost", host);

            // Act
            manager.Navigate<ViewWithDependency>("TestHost");

            // Assert
            host.CurrentContent.Should().NotBeNull();
            host.CurrentContent.Should().BeOfType<ViewWithDependency>();
            
            var view = host.CurrentContent as ViewWithDependency;
            view!.ViewModel.Should().NotBeNull();
        }
    }

    // Test view with dependency
    public class ViewWithDependency : System.Windows.Controls.UserControl
    {
        public TestViewModel ViewModel { get; }

        public ViewWithDependency(TestViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = viewModel;
        }
    }
}

