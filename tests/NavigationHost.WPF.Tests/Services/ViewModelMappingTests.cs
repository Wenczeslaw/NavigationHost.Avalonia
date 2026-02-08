using System.Threading.Tasks;
using System.Windows.Controls;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NavigationHost.Abstractions;
using NavigationHost.WPF.Extensions;
using Xunit;

namespace NavigationHost.WPF.Tests.Services
{
    // Test helper classes
    internal class MappedView : UserControl { }
    internal class MappedViewModel { }
    internal class AnotherMappedView : UserControl { }
    internal class AnotherMappedViewModel { }
    internal class UnconventionalView : UserControl { } // Doesn't follow naming convention
    internal class UnconventionalModel { } // Doesn't follow naming convention

    /// <summary>
    /// Tests for AddView functionality with automatic View-ViewModel mapping.
    /// </summary>
    public class ViewModelMappingTests
    {
        [WpfFact]
        public void AddView_WithViewModel_ShouldRegisterBothInDI()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddNavigationHost()
                .AddView<MappedView, MappedViewModel>();

            var provider = services.BuildServiceProvider();

            // Act
            var view = provider.GetService<MappedView>();
            var viewModel = provider.GetService<MappedViewModel>();

            // Assert - Both View and ViewModel should be resolvable
            view.Should().NotBeNull();
            viewModel.Should().NotBeNull();
        }

        [WpfFact]
        public async Task AddView_WithViewModel_ShouldUseCorrectViewModelInNavigation()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddNavigationHost()
                .AddView<MappedView, MappedViewModel>();

            var provider = services.BuildServiceProvider();
            var hostManager = provider.GetRequiredService<IHostManager>();

            var host = new NavigationHost();
            hostManager.RegisterHost("TestHost", host);

            // Act
            await hostManager.NavigateAsync<MappedView>("TestHost");

            // Assert - View should use the mapped ViewModel
            host.CurrentContent.Should().BeOfType<MappedView>();
            host.CurrentContent!.DataContext.Should().BeOfType<MappedViewModel>();
        }

        [WpfFact]
        public async Task AddView_MultipleViews_ShouldMapEachCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddNavigationHost()
                .AddView<MappedView, MappedViewModel>()
                .AddView<AnotherMappedView, AnotherMappedViewModel>();

            var provider = services.BuildServiceProvider();
            var hostManager = provider.GetRequiredService<IHostManager>();

            var host = new NavigationHost();
            hostManager.RegisterHost("TestHost", host);

            // Act & Assert - First view
            await hostManager.NavigateAsync<MappedView>("TestHost");
            host.CurrentContent.Should().BeOfType<MappedView>();
            host.CurrentContent!.DataContext.Should().BeOfType<MappedViewModel>();

            // Act & Assert - Second view
            await hostManager.NavigateAsync<AnotherMappedView>("TestHost");
            host.CurrentContent.Should().BeOfType<AnotherMappedView>();
            host.CurrentContent!.DataContext.Should().BeOfType<AnotherMappedViewModel>();
        }

        [WpfFact]
        public async Task AddView_UnconventionalNaming_ShouldStillWork()
        {
            // Arrange - Names don't follow View/ViewModel convention
            var services = new ServiceCollection();
            services.AddNavigationHost()
                .AddView<UnconventionalView, UnconventionalModel>();

            var provider = services.BuildServiceProvider();
            var hostManager = provider.GetRequiredService<IHostManager>();

            var host = new NavigationHost();
            hostManager.RegisterHost("TestHost", host);

            // Act
            await hostManager.NavigateAsync<UnconventionalView>("TestHost");

            // Assert - Should use the explicitly mapped ViewModel
            host.CurrentContent.Should().BeOfType<UnconventionalView>();
            host.CurrentContent!.DataContext.Should().BeOfType<UnconventionalModel>();
        }
    }
}
