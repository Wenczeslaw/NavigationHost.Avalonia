using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NavigationHost.WPF.Services.Internal;
using NavigationHost.WPF.Tests.Infrastructure;
using Xunit;

namespace NavigationHost.WPF.Tests.Services.Internal
{
    /// <summary>
    ///     Tests for the InstanceFactory internal service.
    /// </summary>
    public class InstanceFactoryTests
    {
        [WpfFact]
        public void CreateInstance_WithParameterlessConstructor_ShouldCreateInstance()
        {
            // Arrange
            var factory = new InstanceFactory(null);

            // Act
            var instance = factory.CreateInstance(typeof(TestView));

            // Assert
            instance.Should().NotBeNull();
            instance.Should().BeOfType<TestView>();
        }

        [WpfFact]
        public void CreateInstance_Generic_ShouldCreateInstance()
        {
            // Arrange
            var factory = new InstanceFactory(null);

            // Act
            var instance = factory.CreateInstance<TestView>();

            // Assert
            instance.Should().NotBeNull();
            instance.Should().BeOfType<TestView>();
        }

        [WpfFact]
        public void CreateInstance_WithNullType_ShouldThrowArgumentNullException()
        {
            // Arrange
            var factory = new InstanceFactory(null);

            // Act
            Action act = () => factory.CreateInstance(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [WpfFact]
        public void CreateInstance_WithServiceProvider_ShouldResolveFromDI()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTransient<TestView>();
            var provider = services.BuildServiceProvider();
            var factory = new InstanceFactory(provider);

            // Act
            var instance = factory.CreateInstance(typeof(TestView));

            // Assert
            instance.Should().NotBeNull();
            instance.Should().BeOfType<TestView>();
        }

        [WpfFact]
        public void CreateInstance_WithServiceProviderButUnregisteredType_ShouldFallbackToActivator()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();
            var factory = new InstanceFactory(provider);

            // Act
            var instance = factory.CreateInstance(typeof(TestView));

            // Assert
            instance.Should().NotBeNull();
            instance.Should().BeOfType<TestView>();
        }

        [WpfFact]
        public void CreateInstance_WithTypeRequiringDependencies_ShouldResolveFromDI()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTransient<TestViewModel>();
            services.AddTransient<ViewRequiringDependency>();
            var provider = services.BuildServiceProvider();
            var factory = new InstanceFactory(provider);

            // Act
            var instance = factory.CreateInstance(typeof(ViewRequiringDependency));

            // Assert
            instance.Should().NotBeNull();
            instance.Should().BeOfType<ViewRequiringDependency>();
        }

        [WpfFact]
        public void CreateInstance_WithAbstractType_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var factory = new InstanceFactory(null);

            // Act
            Action act = () => factory.CreateInstance(typeof(AbstractView));

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }
    }

    // Test classes for dependency injection
    public class ViewRequiringDependency : System.Windows.Controls.UserControl
    {
        public TestViewModel ViewModel { get; }

        public ViewRequiringDependency(TestViewModel viewModel)
        {
            ViewModel = viewModel;
        }
    }

    public abstract class AbstractView : System.Windows.Controls.UserControl
    {
    }
}

