using Avalonia.Controls;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NavigationHost.Abstractions;
using NavigationHost.Avalonia.Extensions;

namespace NavigationHost.Avalonia.Tests.Services
{
    // Test helper classes
    internal class MappedView : Control { }
    internal class MappedViewModel { }
    internal class AnotherMappedView : Control { }
    internal class AnotherMappedViewModel { }
    internal class UnconventionalView : Control { } // Doesn't follow naming convention
    internal class UnconventionalModel { } // Doesn't follow naming convention

    /// <summary>
    /// Tests for AddView functionality with automatic View-ViewModel mapping.
    /// </summary>
    public class ViewModelMappingTests
    {
        [Fact]
        public void AddView_WithViewModel_ShouldRegisterBothInDI()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddHostManager()
                .AddView<MappedView, MappedViewModel>();

            var provider = services.BuildServiceProvider();

            // Act
            var view = provider.GetService<MappedView>();
            var viewModel = provider.GetService<MappedViewModel>();

            // Assert - Both View and ViewModel should be resolvable
            view.Should().NotBeNull();
            viewModel.Should().NotBeNull();
        }

        [Fact]
        public async Task AddView_WithViewModel_ShouldUseCorrectViewModelInNavigation()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddHostManager()
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

        [Fact]
        public async Task AddView_MultipleViews_ShouldMapEachCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddHostManager()
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

        [Fact]
        public async Task AddView_UnconventionalNaming_ShouldStillWork()
        {
            // Arrange - Names don't follow View/ViewModel convention
            var services = new ServiceCollection();
            services.AddHostManager()
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
