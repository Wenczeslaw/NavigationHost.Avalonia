using System;
using Avalonia.Controls;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NavigationHost.Abstractions;
using NavigationHost.Avalonia.Extensions;

// Test namespace patterns
namespace MyApp.Views
{
    public class ProductView : Control { }
}

namespace MyApp.ViewModels
{
    public class ProductViewModel { }
    public class UserViewModel { } // For nested Views.Admin.UserView
}

namespace Views
{
    public class CustomerView : Control { }
}

namespace ViewModels
{
    public class CustomerViewModel { }
}

namespace Company.App.Views
{
    public class OrderView : Control { }
}

namespace Company.App.ViewModels
{
    public class OrderViewModel { }
}

namespace MyApp.Views.Admin
{
    public class UserView : Control { }
}

namespace MyApp.Modules.Sales.Views
{
    public class InvoiceView : Control { }
}

namespace MyApp.Modules.Sales.ViewModels
{
    public class InvoiceViewModel { }
}

namespace NavigationHost.Avalonia.Tests.Services
{
    /// <summary>
    /// Tests for convention-based View-ViewModel resolution with different namespace patterns.
    /// </summary>
    public class ViewModelConventionResolverTests
    {
        private readonly IViewModelConventionResolver _resolver;
        private readonly IServiceProvider _serviceProvider;

        public ViewModelConventionResolverTests()
        {
            var services = new ServiceCollection();
            services.AddHostManager();
            _serviceProvider = services.BuildServiceProvider();
            _resolver = _serviceProvider.GetRequiredService<IViewModelConventionResolver>();
        }

        [Fact]
        public void ResolveViewModelType_Pattern1_MyAppViewsToMyAppViewModels()
        {
            // Arrange
            var viewType = typeof(MyApp.Views.ProductView);

            // Act
            var viewModelType = _resolver.ResolveViewModelType(viewType);

            // Assert
            viewModelType.Should().NotBeNull();
            viewModelType.Should().Be(typeof(MyApp.ViewModels.ProductViewModel));
        }

        [Fact]
        public void ResolveViewModelType_Pattern2_ViewsToViewModels()
        {
            // Arrange
            var viewType = typeof(Views.CustomerView);

            // Act
            var viewModelType = _resolver.ResolveViewModelType(viewType);

            // Assert
            viewModelType.Should().NotBeNull();
            viewModelType.Should().Be(typeof(ViewModels.CustomerViewModel));
        }

        [Fact]
        public void ResolveViewModelType_Pattern3_CompanyAppViewsToCompanyAppViewModels()
        {
            // Arrange
            var viewType = typeof(Company.App.Views.OrderView);

            // Act
            var viewModelType = _resolver.ResolveViewModelType(viewType);

            // Assert
            viewModelType.Should().NotBeNull();
            viewModelType.Should().Be(typeof(Company.App.ViewModels.OrderViewModel));
        }

        [Fact]
        public void ResolveViewModelType_Pattern4_NestedViewsToRootViewModels()
        {
            // Arrange
            var viewType = typeof(MyApp.Views.Admin.UserView);

            // Act
            var viewModelType = _resolver.ResolveViewModelType(viewType);

            // Assert
            viewModelType.Should().NotBeNull();
            viewModelType.Should().Be(typeof(MyApp.ViewModels.UserViewModel));
        }

        [Fact]
        public void ResolveViewModelType_Pattern5_ModuleViewsToModuleViewModels()
        {
            // Arrange
            var viewType = typeof(MyApp.Modules.Sales.Views.InvoiceView);

            // Act
            var viewModelType = _resolver.ResolveViewModelType(viewType);

            // Assert
            viewModelType.Should().NotBeNull();
            viewModelType.Should().Be(typeof(MyApp.Modules.Sales.ViewModels.InvoiceViewModel));
        }

        [Fact]
        public void CanResolve_WithValidConvention_ShouldReturnTrue()
        {
            // Arrange
            var viewType = typeof(MyApp.Views.ProductView);

            // Act
            var canResolve = _resolver.CanResolve(viewType);

            // Assert
            canResolve.Should().BeTrue();
        }

        [Fact]
        public void CanResolve_WithInvalidConvention_ShouldReturnFalse()
        {
            // Arrange - A view type that doesn't follow any convention or has no matching ViewModel
            var viewType = typeof(Control);

            // Act
            var canResolve = _resolver.CanResolve(viewType);

            // Assert
            canResolve.Should().BeFalse();
        }

        [Fact]
        public void ResolveViewModelType_WithNullViewType_ShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => _resolver.ResolveViewModelType(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
