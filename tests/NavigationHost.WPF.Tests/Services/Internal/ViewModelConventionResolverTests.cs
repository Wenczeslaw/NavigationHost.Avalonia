using System;
using FluentAssertions;
using NavigationHost.WPF.Services.Internal;
using NavigationHost.WPF.Tests.Infrastructure;
using Xunit;

namespace NavigationHost.WPF.Tests.Services.Internal
{
    /// <summary>
    ///     Tests for the ViewModelConventionResolver internal service.
    /// </summary>
    public class ViewModelConventionResolverTests
    {
        private readonly ViewModelConventionResolver _resolver;

        public ViewModelConventionResolverTests()
        {
            _resolver = new ViewModelConventionResolver();
        }

        [Fact]
        public void ResolveViewModelType_WithValidViewType_ShouldResolveViewModel()
        {
            // Arrange
            var viewType = typeof(TestView);

            // Act
            var viewModelType = _resolver.ResolveViewModelType(viewType);

            // Assert
            viewModelType.Should().NotBeNull();
            viewModelType.Should().Be(typeof(TestViewModel));
        }

        [Fact]
        public void ResolveViewModelType_WithNullViewType_ShouldReturnNull()
        {
            // Act
            var viewModelType = _resolver.ResolveViewModelType(null!);

            // Assert
            viewModelType.Should().BeNull();
        }

        [Fact]
        public void ResolveViewModelType_WithViewNotEndingInView_ShouldReturnNull()
        {
            // Arrange
            var viewType = typeof(AnotherTestViewModel);

            // Act
            var viewModelType = _resolver.ResolveViewModelType(viewType);

            // Assert
            viewModelType.Should().BeNull();
        }

        [Fact]
        public void ResolveViewModelType_WithNonExistentViewModel_ShouldReturnNull()
        {
            // Arrange
            var viewType = typeof(ViewWithoutViewModel);

            // Act
            var viewModelType = _resolver.ResolveViewModelType(viewType);

            // Assert
            viewModelType.Should().BeNull();
        }

        [Fact]
        public void CanResolve_WithValidViewType_ShouldReturnTrue()
        {
            // Arrange
            var viewType = typeof(TestView);

            // Act
            var canResolve = _resolver.CanResolve(viewType);

            // Assert
            canResolve.Should().BeTrue();
        }

        [Fact]
        public void CanResolve_WithInvalidViewType_ShouldReturnFalse()
        {
            // Arrange
            var viewType = typeof(ViewWithoutViewModel);

            // Act
            var canResolve = _resolver.CanResolve(viewType);

            // Assert
            canResolve.Should().BeFalse();
        }

        [Fact]
        public void CanResolve_WithNullViewType_ShouldReturnFalse()
        {
            // Act
            var canResolve = _resolver.CanResolve(null!);

            // Assert
            canResolve.Should().BeFalse();
        }
    }

    // Test class that doesn't follow the naming convention
    public class ViewWithoutViewModel : System.Windows.Controls.UserControl
    {
    }
}

