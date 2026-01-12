using FluentAssertions;
using NavigationHost.WPF.Tests.Infrastructure;
using Xunit;

namespace NavigationHost.WPF.Tests.Abstractions
{
    /// <summary>
    ///     Tests for INavigationAware interface implementation.
    /// </summary>
    public class NavigationAwareTests
    {
        [Fact]
        public void CanNavigateTo_ShouldBeCalledBeforeNavigation()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var parameter = new { Id = 123 };

            // Act
            var canNavigate = viewModel.CanNavigateTo(parameter);

            // Assert
            viewModel.CanNavigateToWasCalled.Should().BeTrue();
            canNavigate.Should().BeTrue();
            viewModel.ReceivedParameter.Should().Be(parameter);
        }

        [Fact]
        public void CanNavigateTo_WhenReturnsFalse_ShouldPreventNavigation()
        {
            // Arrange
            var viewModel = new TestViewModel
            {
                AllowNavigateTo = false
            };

            // Act
            var canNavigate = viewModel.CanNavigateTo(null);

            // Assert
            canNavigate.Should().BeFalse();
        }

        [Fact]
        public void OnNavigatedTo_ShouldReceiveParameter()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var parameter = new { Id = 456 };

            // Act
            viewModel.OnNavigatedTo(parameter);

            // Assert
            viewModel.OnNavigatedToWasCalled.Should().BeTrue();
            viewModel.ReceivedParameter.Should().Be(parameter);
        }

        [Fact]
        public void CanNavigateFrom_ShouldBeCalledBeforeLeavingView()
        {
            // Arrange
            var viewModel = new TestViewModel();

            // Act
            var canNavigate = viewModel.CanNavigateFrom();

            // Assert
            viewModel.CanNavigateFromWasCalled.Should().BeTrue();
            canNavigate.Should().BeTrue();
        }

        [Fact]
        public void CanNavigateFrom_WhenReturnsFalse_ShouldPreventNavigationAway()
        {
            // Arrange
            var viewModel = new TestViewModel
            {
                AllowNavigateFrom = false
            };

            // Act
            var canNavigate = viewModel.CanNavigateFrom();

            // Assert
            canNavigate.Should().BeFalse();
        }

        [Fact]
        public void OnNavigatedFrom_ShouldBeCalledWhenLeavingView()
        {
            // Arrange
            var viewModel = new TestViewModel();

            // Act
            viewModel.OnNavigatedFrom();

            // Assert
            viewModel.OnNavigatedFromWasCalled.Should().BeTrue();
        }

        [Fact]
        public void NavigationLifecycle_ShouldFollowCorrectOrder()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var parameter = "test";

            // Act & Assert - Simulate navigation lifecycle

            // Step 1: Check if can navigate to
            viewModel.CanNavigateTo(parameter).Should().BeTrue();
            viewModel.CanNavigateToWasCalled.Should().BeTrue();
            viewModel.OnNavigatedToWasCalled.Should().BeFalse();

            // Step 2: Navigate to
            viewModel.OnNavigatedTo(parameter);
            viewModel.OnNavigatedToWasCalled.Should().BeTrue();
            viewModel.ReceivedParameter.Should().Be(parameter);

            // Step 3: Check if can navigate from
            viewModel.CanNavigateFrom().Should().BeTrue();
            viewModel.CanNavigateFromWasCalled.Should().BeTrue();
            viewModel.OnNavigatedFromWasCalled.Should().BeFalse();

            // Step 4: Navigate from
            viewModel.OnNavigatedFrom();
            viewModel.OnNavigatedFromWasCalled.Should().BeTrue();
        }
    }
}
