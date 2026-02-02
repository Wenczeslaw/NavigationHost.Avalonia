using Avalonia.Controls;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NavigationHost.Abstractions;
using NavigationHost.Avalonia.Extensions;
using NavigationHost.Avalonia.Services;

namespace NavigationHost.Avalonia.Tests.Services
{
    // Test helper classes for async navigation testing
    internal class AsyncTestView : Control { }
    internal class AsyncTestViewModel : IAsyncNavigationAware
    {
        public bool CanNavigateToResult { get; set; } = true;
        public bool CanNavigateFromResult { get; set; } = true;
        public object? ReceivedParameter { get; private set; }
        public int OnNavigatedToCallCount { get; private set; }
        public int OnNavigatedFromCallCount { get; private set; }
        public int CanNavigateToCallCount { get; private set; }
        public int CanNavigateFromCallCount { get; private set; }
        public int AsyncDelayMs { get; set; } = 0;

        public async Task<bool> CanNavigateToAsync(object? parameter)
        {
            CanNavigateToCallCount++;
            ReceivedParameter = parameter;
            if (AsyncDelayMs > 0)
            {
                await Task.Delay(AsyncDelayMs);
            }
            return CanNavigateToResult;
        }

        public async Task OnNavigatedToAsync(object? parameter)
        {
            OnNavigatedToCallCount++;
            ReceivedParameter = parameter;
            if (AsyncDelayMs > 0)
            {
                await Task.Delay(AsyncDelayMs);
            }
        }

        public async Task<bool> CanNavigateFromAsync()
        {
            CanNavigateFromCallCount++;
            if (AsyncDelayMs > 0)
            {
                await Task.Delay(AsyncDelayMs);
            }
            return CanNavigateFromResult;
        }

        public async Task OnNavigatedFromAsync()
        {
            OnNavigatedFromCallCount++;
            if (AsyncDelayMs > 0)
            {
                await Task.Delay(AsyncDelayMs);
            }
        }
    }

    internal class ProductView : Control { }
    internal class ProductViewModel : IAsyncNavigationAware
    {
        public int? ProductId { get; private set; }
        public bool IsLoading { get; set; }
        public bool LoadDataCalled { get; private set; }
        public Exception? LoadingException { get; set; }

        public Task<bool> CanNavigateToAsync(object? parameter)
        {
            // Validate parameter type
            return Task.FromResult(parameter is int or null);
        }

        public async Task OnNavigatedToAsync(object? parameter)
        {
            IsLoading = true;
            try
            {
                if (parameter is int productId)
                {
                    ProductId = productId;
                    await LoadProductDataAsync(productId);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        public Task<bool> CanNavigateFromAsync()
        {
            return Task.FromResult(true);
        }

        public Task OnNavigatedFromAsync()
        {
            ProductId = null;
            return Task.CompletedTask;
        }

        private async Task LoadProductDataAsync(int productId)
        {
            await Task.Delay(10); // Simulate API call
            
            if (LoadingException != null)
            {
                throw LoadingException;
            }
            
            LoadDataCalled = true;
        }
    }

    internal class MixedViewModel : INavigationAware, IAsyncNavigationAware
    {
        public bool SyncCalled { get; private set; }
        public bool AsyncCalled { get; private set; }

        // INavigationAware implementation
        public bool CanNavigateTo(object? parameter)
        {
            SyncCalled = true;
            return true;
        }

        public void OnNavigatedTo(object? parameter)
        {
            SyncCalled = true;
        }

        public bool CanNavigateFrom()
        {
            SyncCalled = true;
            return true;
        }

        public void OnNavigatedFrom()
        {
            SyncCalled = true;
        }

        // IAsyncNavigationAware implementation
        public Task<bool> CanNavigateToAsync(object? parameter)
        {
            AsyncCalled = true;
            return Task.FromResult(true);
        }

        public Task OnNavigatedToAsync(object? parameter)
        {
            AsyncCalled = true;
            return Task.CompletedTask;
        }

        public Task<bool> CanNavigateFromAsync()
        {
            AsyncCalled = true;
            return Task.FromResult(true);
        }

        public Task OnNavigatedFromAsync()
        {
            AsyncCalled = true;
            return Task.CompletedTask;
        }
    }

    internal class MixedView : Control { }

    public class HostManagerAsyncTests
    {
        private readonly IHostManager _sut;
        private readonly IServiceProvider _serviceProvider;

        public HostManagerAsyncTests()
        {
            var services = new ServiceCollection();
            
            // Register navigation services
            services.AddHostManager();
            
            // Register test views and view models
            services.AddTransient<AsyncTestView>();
            services.AddTransient<AsyncTestViewModel>();
            services.AddTransient<ProductView>();
            services.AddTransient<ProductViewModel>();
            services.AddTransient<MixedView>();
            services.AddTransient<MixedViewModel>();
            
            _serviceProvider = services.BuildServiceProvider();
            _sut = _serviceProvider.GetRequiredService<IHostManager>();
        }

        #region Basic Async Navigation Tests

        [Fact]
        public async Task NavigateAsync_WithAsyncViewModel_ShouldCompleteSuccessfully()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);

            // Act
            await _sut.NavigateAsync<AsyncTestView>("TestHost");

            // Assert
            host.CurrentContent.Should().NotBeNull();
            host.CurrentContent.Should().BeOfType<AsyncTestView>();
            host.CurrentContent!.DataContext.Should().BeOfType<AsyncTestViewModel>();
        }

        [Fact]
        public async Task NavigateAsync_WithParameter_ShouldPassParameterToViewModel()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);
            var parameter = 12345;

            // Act
            await _sut.NavigateAsync<AsyncTestView>("TestHost", parameter: parameter);

            // Assert
            var viewModel = host.CurrentContent!.DataContext as AsyncTestViewModel;
            viewModel.Should().NotBeNull();
            viewModel!.ReceivedParameter.Should().Be(parameter);
            viewModel.OnNavigatedToCallCount.Should().Be(1);
        }

        [Fact]
        public async Task NavigateAsync_WithComplexParameter_ShouldPassObject()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);
            var parameter = new { Id = 123, Name = "Test Product" };

            // Act
            await _sut.NavigateAsync<AsyncTestView>("TestHost", parameter: parameter);

            // Assert
            var viewModel = host.CurrentContent!.DataContext as AsyncTestViewModel;
            viewModel.Should().NotBeNull();
            viewModel!.ReceivedParameter.Should().Be(parameter);
        }

        [Fact]
        public async Task NavigateAsync_WithNonRegisteredHost_ShouldThrowInvalidOperationException()
        {
            // Act
            Func<Task> act = async () => await _sut.NavigateAsync<AsyncTestView>("NonExistingHost");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("No host registered with name 'NonExistingHost'.");
        }

        #endregion

        #region Lifecycle Method Tests

        [Fact]
        public async Task NavigateAsync_ShouldCallCanNavigateToAsync()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);

            // Act
            await _sut.NavigateAsync<AsyncTestView>("TestHost");

            // Assert
            var viewModel = host.CurrentContent!.DataContext as AsyncTestViewModel;
            viewModel!.CanNavigateToCallCount.Should().Be(1);
        }

        [Fact]
        public async Task NavigateAsync_WhenCanNavigateToAsyncReturnsFalse_ShouldCancelNavigation()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);
            
            var initialView = new Control();
            host.Navigate(initialView);

            // Create a service provider with custom ViewModel that returns false
            var services = new ServiceCollection();
            services.AddHostManager();
            services.AddTransient<AsyncTestView>();
            services.AddTransient<AsyncTestViewModel>(sp =>
            {
                return new AsyncTestViewModel { CanNavigateToResult = false };
            });
            
            var provider = services.BuildServiceProvider();
            var hostManager = provider.GetRequiredService<IHostManager>();
            hostManager.RegisterHost("TestHost", host);

            // Act
            await hostManager.NavigateAsync<AsyncTestView>("TestHost");

            // Assert
            host.CurrentContent.Should().BeSameAs(initialView); // Navigation cancelled
        }

        [Fact]
        public async Task NavigateAsync_ShouldCallOnNavigatedToAsync()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);

            // Act
            await _sut.NavigateAsync<AsyncTestView>("TestHost");

            // Assert
            var viewModel = host.CurrentContent!.DataContext as AsyncTestViewModel;
            viewModel!.OnNavigatedToCallCount.Should().Be(1);
        }

        [Fact]
        public async Task NavigateAsync_WhenNavigatingAway_ShouldCallCanNavigateFromAsync()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);
            
            // First navigation
            await _sut.NavigateAsync<AsyncTestView>("TestHost");
            var firstViewModel = host.CurrentContent!.DataContext as AsyncTestViewModel;

            // Act - Second navigation
            await _sut.NavigateAsync<ProductView>("TestHost");

            // Assert
            firstViewModel!.CanNavigateFromCallCount.Should().Be(1);
        }

        [Fact]
        public async Task NavigateAsync_WhenCanNavigateFromAsyncReturnsFalse_ShouldCancelNavigation()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);
            
            var services = new ServiceCollection();
            services.AddHostManager();
            services.AddTransient<AsyncTestView>();
            services.AddTransient<AsyncTestViewModel>(sp =>
            {
                return new AsyncTestViewModel { CanNavigateFromResult = false };
            });
            services.AddTransient<ProductView>();
            services.AddTransient<ProductViewModel>();
            
            var provider = services.BuildServiceProvider();
            var hostManager = provider.GetRequiredService<IHostManager>();
            hostManager.RegisterHost("TestHost", host);
            
            // First navigation
            await hostManager.NavigateAsync<AsyncTestView>("TestHost");
            var firstView = host.CurrentContent;

            // Act - Try to navigate away
            await hostManager.NavigateAsync<ProductView>("TestHost");

            // Assert
            host.CurrentContent.Should().BeSameAs(firstView); // Navigation cancelled
        }

        [Fact]
        public async Task NavigateAsync_WhenNavigatingAway_ShouldCallOnNavigatedFromAsync()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);
            
            // First navigation
            await _sut.NavigateAsync<AsyncTestView>("TestHost");
            var firstViewModel = host.CurrentContent!.DataContext as AsyncTestViewModel;

            // Act - Second navigation
            await _sut.NavigateAsync<ProductView>("TestHost");

            // Assert
            firstViewModel!.OnNavigatedFromCallCount.Should().Be(1);
        }

        #endregion

        #region Real-World Scenario Tests

        [Fact]
        public async Task NavigateAsync_WithProductViewModel_ShouldLoadDataAsync()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);
            var productId = 42;

            // Act
            await _sut.NavigateAsync<ProductView>("TestHost", parameter: productId);

            // Assert
            var viewModel = host.CurrentContent!.DataContext as ProductViewModel;
            viewModel.Should().NotBeNull();
            viewModel!.ProductId.Should().Be(productId);
            viewModel.LoadDataCalled.Should().BeTrue();
            viewModel.IsLoading.Should().BeFalse(); // Loading should be complete
        }

        [Fact]
        public async Task NavigateAsync_WithInvalidParameter_ShouldNotNavigate()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);
            var initialView = new Control();
            host.Navigate(initialView);
            
            var invalidParameter = "not an int"; // ProductViewModel expects int or null

            // Act
            await _sut.NavigateAsync<ProductView>("TestHost", parameter: invalidParameter);

            // Assert - Navigation should be cancelled
            host.CurrentContent.Should().BeSameAs(initialView);
        }

        [Fact]
        public async Task NavigateAsync_WithDelay_ShouldCompleteAfterDelay()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);
            
            var services = new ServiceCollection();
            services.AddHostManager();
            services.AddTransient<AsyncTestView>();
            services.AddTransient<AsyncTestViewModel>(sp =>
            {
                return new AsyncTestViewModel { AsyncDelayMs = 100 };
            });
            
            var provider = services.BuildServiceProvider();
            var hostManager = provider.GetRequiredService<IHostManager>();
            hostManager.RegisterHost("TestHost", host);

            // Act
            var startTime = DateTime.UtcNow;
            await hostManager.NavigateAsync<AsyncTestView>("TestHost");
            var elapsed = DateTime.UtcNow - startTime;

            // Assert
            elapsed.TotalMilliseconds.Should().BeGreaterThanOrEqualTo(100);
            host.CurrentContent.Should().BeOfType<AsyncTestView>();
        }

        #endregion

        #region Mixed Interface Tests

        [Fact]
        public async Task NavigateAsync_WithMixedViewModel_ShouldPreferAsyncInterface()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);

            // Act
            await _sut.NavigateAsync<MixedView>("TestHost");

            // Assert
            var viewModel = host.CurrentContent!.DataContext as MixedViewModel;
            viewModel.Should().NotBeNull();
            viewModel!.AsyncCalled.Should().BeTrue();
            viewModel.SyncCalled.Should().BeFalse(); // Sync should not be called when async is available
        }

        [Fact]
        public void Navigate_WithMixedViewModel_ShouldUseSyncInterface()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);

            // Act - Use synchronous Navigate
            _sut.Navigate<MixedView>("TestHost");

            // Assert
            var viewModel = host.CurrentContent!.DataContext as MixedViewModel;
            viewModel.Should().NotBeNull();
            viewModel!.SyncCalled.Should().BeTrue();
            viewModel.AsyncCalled.Should().BeFalse();
        }

        #endregion

        #region Multiple Navigation Tests

        [Fact]
        public async Task NavigateAsync_MultipleTimes_ShouldMaintainLifecycle()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);

            // First navigation
            await _sut.NavigateAsync<AsyncTestView>("TestHost", parameter: 1);
            var firstViewModel = host.CurrentContent!.DataContext as AsyncTestViewModel;

            // Second navigation
            await _sut.NavigateAsync<ProductView>("TestHost", parameter: 2);
            var secondViewModel = host.CurrentContent!.DataContext as ProductViewModel;

            // Third navigation
            await _sut.NavigateAsync<AsyncTestView>("TestHost", parameter: 3);
            var thirdViewModel = host.CurrentContent!.DataContext as AsyncTestViewModel;

            // Assert
            firstViewModel!.CanNavigateToCallCount.Should().Be(1);
            firstViewModel.OnNavigatedToCallCount.Should().Be(1);
            firstViewModel.CanNavigateFromCallCount.Should().Be(1);
            firstViewModel.OnNavigatedFromCallCount.Should().Be(1);

            // ProductViewModel loads data async, so we need to check it loaded correctly
            secondViewModel.Should().NotBeNull();
            secondViewModel!.LoadDataCalled.Should().BeTrue();
            
            thirdViewModel!.ReceivedParameter.Should().Be(3);
            thirdViewModel.CanNavigateToCallCount.Should().Be(1);
            thirdViewModel.OnNavigatedToCallCount.Should().Be(1);
        }

        [Fact]
        public async Task NavigateAsync_ToSameView_ShouldCreateNewInstance()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);

            // First navigation
            await _sut.NavigateAsync<AsyncTestView>("TestHost", parameter: 1);
            var firstView = host.CurrentContent;
            var firstViewModel = firstView!.DataContext;

            // Second navigation to same view type
            await _sut.NavigateAsync<AsyncTestView>("TestHost", parameter: 2);
            var secondView = host.CurrentContent;
            var secondViewModel = secondView!.DataContext;

            // Assert - Should be different instances (Transient lifetime)
            firstView.Should().NotBeSameAs(secondView);
            firstViewModel.Should().NotBeSameAs(secondViewModel);
        }

        #endregion

        #region Type-Based Navigation Tests

        [Fact]
        public async Task NavigateAsync_WithType_ShouldNavigate()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);

            // Act
            await _sut.NavigateAsync("TestHost", typeof(AsyncTestView), parameter: 123);

            // Assert
            host.CurrentContent.Should().BeOfType<AsyncTestView>();
            var viewModel = host.CurrentContent!.DataContext as AsyncTestViewModel;
            viewModel.Should().NotBeNull();
            viewModel!.ReceivedParameter.Should().Be(123);
        }

        [Fact]
        public async Task NavigateAsync_WithContentInstance_ShouldNavigate()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);
            var content = new AsyncTestView
            {
                DataContext = new AsyncTestViewModel()
            };

            // Act
            await _sut.NavigateAsync("TestHost", content);

            // Assert
            host.CurrentContent.Should().BeSameAs(content);
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task NavigateAsync_WhenExceptionInOnNavigatedTo_ShouldPropagate()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);
            
            var services = new ServiceCollection();
            services.AddHostManager();
            services.AddTransient<ProductView>();
            services.AddTransient<ProductViewModel>(sp =>
            {
                return new ProductViewModel 
                { 
                    LoadingException = new InvalidOperationException("Test exception") 
                };
            });
            
            var provider = services.BuildServiceProvider();
            var hostManager = provider.GetRequiredService<IHostManager>();
            hostManager.RegisterHost("TestHost", host);

            // Act
            Func<Task> act = async () => await hostManager.NavigateAsync<ProductView>("TestHost", parameter: 1);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Test exception");
        }

        #endregion

        #region Concurrency Tests

        [Fact]
        public async Task NavigateAsync_ConcurrentNavigations_ShouldHandleGracefully()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);

            // Act - Start multiple navigations concurrently
            var task1 = _sut.NavigateAsync<AsyncTestView>("TestHost", parameter: 1);
            var task2 = _sut.NavigateAsync<ProductView>("TestHost", parameter: 2);
            var task3 = _sut.NavigateAsync<AsyncTestView>("TestHost", parameter: 3);

            await Task.WhenAll(task1, task2, task3);

            // Assert - Last navigation should win
            host.CurrentContent.Should().NotBeNull();
            // The final state depends on which task completed last
        }

        #endregion

        #region Null and Edge Cases

        [Fact]
        public async Task NavigateAsync_WithNullParameter_ShouldNotThrow()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);

            // Act
            Func<Task> act = async () => await _sut.NavigateAsync<AsyncTestView>("TestHost", parameter: null);

            // Assert
            await act.Should().NotThrowAsync();
            host.CurrentContent.Should().BeOfType<AsyncTestView>();
        }

        [Fact]
        public async Task NavigateAsync_WithNullContentType_ShouldThrowArgumentNullException()
        {
            // Arrange
            var host = new NavigationHost();
            _sut.RegisterHost("TestHost", host);

            // Act
            Func<Task> act = async () => await _sut.NavigateAsync("TestHost", (Type)null!, parameter: null);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task NavigateAsync_WithEmptyHostName_ShouldThrowInvalidOperationException()
        {
            // Act
            Func<Task> act = async () => await _sut.NavigateAsync<AsyncTestView>("");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        #endregion
    }
}
