using System;
using System.Windows.Controls;
using FluentAssertions;
using NavigationHost.WPF.Tests.Infrastructure;
using Xunit;

namespace NavigationHost.WPF.Tests.Controls
{
    /// <summary>
    ///     Tests for the NavigationHost control.
    /// </summary>
    public class NavigationHostTests : TestFixtureBase
    {
        [WpfFact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange & Act
            var host = new NavigationHost();

            // Assert
            host.Should().NotBeNull();
            host.CurrentContent.Should().BeNull();
            host.DefaultContent.Should().BeNull();
        }

        [WpfFact]
        public void Navigate_WithValidContent_ShouldSetCurrentContent()
        {
            // Arrange
            var host = new NavigationHost();
            var view = new TestView();

            // Act
            host.Navigate(view);

            // Assert
            host.CurrentContent.Should().BeSameAs(view);
        }

        [WpfFact]
        public void Navigate_WithNullContent_ShouldThrowArgumentNullException()
        {
            // Arrange
            var host = new NavigationHost();

            // Act
            Action act = () => host.Navigate(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("content");
        }

        [WpfFact]
        public void Navigate_ShouldRaiseNavigatedEvent()
        {
            // Arrange
            var host = new NavigationHost();
            var view = new TestView();
            var eventRaised = false;
            Events.NavigationEventArgs? eventArgs = null;

            host.Navigated += (sender, args) =>
            {
                eventRaised = true;
                eventArgs = args;
            };

            // Act
            host.Navigate(view);

            // Assert
            eventRaised.Should().BeTrue();
            eventArgs.Should().NotBeNull();
            eventArgs!.Content.Should().BeSameAs(view);
            eventArgs.PreviousContent.Should().BeNull();
        }

        [WpfFact]
        public void Navigate_Twice_ShouldUpdatePreviousContent()
        {
            // Arrange
            var host = new NavigationHost();
            var firstView = new TestView();
            var secondView = new AnotherTestView();
            
            host.Navigate(firstView);

            Events.NavigationEventArgs? eventArgs = null;
            host.Navigated += (sender, args) =>
            {
                eventArgs = args;
            };

            // Act
            host.Navigate(secondView);

            // Assert
            host.CurrentContent.Should().BeSameAs(secondView);
            eventArgs.Should().NotBeNull();
            eventArgs!.Content.Should().BeSameAs(secondView);
            eventArgs.PreviousContent.Should().BeSameAs(firstView);
        }

        [WpfFact]
        public void DefaultContent_WhenSet_ShouldNavigateToIt()
        {
            // Arrange
            var host = new NavigationHost();
            var defaultView = new TestView();

            // Act
            host.DefaultContent = defaultView;

            // Assert
            host.CurrentContent.Should().BeSameAs(defaultView);
        }

        [WpfFact]
        public void DefaultContent_WhenCurrentContentAlreadySet_ShouldNotOverride()
        {
            // Arrange
            var host = new NavigationHost();
            var currentView = new TestView();
            var defaultView = new AnotherTestView();

            host.CurrentContent = currentView;

            // Act
            host.DefaultContent = defaultView;

            // Assert
            host.CurrentContent.Should().BeSameAs(currentView);
        }

        [WpfFact]
        public void Content_ShouldReflectCurrentContent()
        {
            // Arrange
            var host = new NavigationHost();
            var view = new TestView();

            // Act
            host.Navigate(view);

            // Assert
            host.Content.Should().BeSameAs(view);
        }
    }
}

