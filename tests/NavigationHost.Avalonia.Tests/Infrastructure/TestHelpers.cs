using Avalonia.Controls;
using FluentAssertions;
using NavigationHost.Avalonia.Abstractions;

namespace NavigationHost.Avalonia.Tests.Infrastructure
{
    /// <summary>
    /// Utilities for testing navigation scenarios.
    /// </summary>
    public static class TestHelpers
    {
        /// <summary>
        /// Creates a test parameter object with the specified value.
        /// </summary>
        public static object CreateTestParameter(string value) => new { Value = value };

        /// <summary>
        /// Waits for a condition to be true or times out.
        /// </summary>
        public static async Task<bool> WaitForConditionAsync(
            Func<bool> condition,
            TimeSpan timeout,
            TimeSpan? pollingInterval = null)
        {
            var interval = pollingInterval ?? TimeSpan.FromMilliseconds(10);
            var endTime = DateTime.UtcNow + timeout;

            while (DateTime.UtcNow < endTime)
            {
                if (condition())
                    return true;

                await Task.Delay(interval);
            }

            return false;
        }

        /// <summary>
        /// Executes an action multiple times concurrently.
        /// </summary>
        public static async Task ExecuteConcurrentlyAsync(Action action, int concurrency)
        {
            var tasks = Enumerable.Range(0, concurrency)
                .Select(_ => Task.Run(action))
                .ToArray();

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Measures the execution time of an action.
        /// </summary>
        public static TimeSpan MeasureExecutionTime(Action action)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        /// <summary>
        /// Gets the current thread ID.
        /// </summary>
        public static int GetCurrentThreadId() => Thread.CurrentThread.ManagedThreadId;
    }

    /// <summary>
    /// Custom assertions for navigation testing.
    /// </summary>
    public static class NavigationAssertions
    {
        /// <summary>
        /// Asserts that a host has the expected current content.
        /// </summary>
        public static void ShouldHaveContent<TView>(
            this NavigationHost navigationHost)
            where TView : Control
        {
            navigationHost.CurrentContent.Should().NotBeNull("host should have current content");
            navigationHost.CurrentContent.Should().BeOfType<TView>();
        }

        /// <summary>
        /// Asserts that a host's current content has the expected viewmodel.
        /// </summary>
        public static void ShouldHaveViewModel<TViewModel>(
            this NavigationHost navigationHost)
        {
            navigationHost.CurrentContent.Should().NotBeNull("host should have current content");
            navigationHost.CurrentContent!.DataContext.Should().NotBeNull("content should have DataContext");
            navigationHost.CurrentContent.DataContext.Should().BeOfType<TViewModel>();
        }

        /// <summary>
        /// Asserts that a host has both the expected view and viewmodel.
        /// </summary>
        public static void ShouldHaveViewAndViewModel<TView, TViewModel>(
            this NavigationHost navigationHost)
            where TView : Control
        {
            navigationHost.ShouldHaveContent<TView>();
            navigationHost.ShouldHaveViewModel<TViewModel>();
        }

        /// <summary>
        /// Asserts that a NavigationAware viewmodel has the expected lifecycle call counts.
        /// </summary>
        public static void ShouldHaveLifecycleCounts(
            this INavigationAware viewModel,
            int? canNavigateTo = null,
            int? onNavigatedTo = null,
            int? canNavigateFrom = null,
            int? onNavigatedFrom = null)
        {
            if (viewModel is NavigationAwareTestViewModel testVm)
            {
                if (canNavigateTo.HasValue)
                    testVm.CanNavigateToCallCount.Should().Be(canNavigateTo.Value,
                        "CanNavigateTo should be called the expected number of times");

                if (onNavigatedTo.HasValue)
                    testVm.OnNavigatedToCallCount.Should().Be(onNavigatedTo.Value,
                        "OnNavigatedTo should be called the expected number of times");

                if (canNavigateFrom.HasValue)
                    testVm.CanNavigateFromCallCount.Should().Be(canNavigateFrom.Value,
                        "CanNavigateFrom should be called the expected number of times");

                if (onNavigatedFrom.HasValue)
                    testVm.OnNavigatedFromCallCount.Should().Be(onNavigatedFrom.Value,
                        "OnNavigatedFrom should be called the expected number of times");
            }
        }

        /// <summary>
        /// Asserts that a NavigationAware viewmodel received the expected parameter.
        /// </summary>
        public static void ShouldHaveReceivedParameter(
            this INavigationAware viewModel,
            object? expectedParameter)
        {
            if (viewModel is NavigationAwareTestViewModel testVm)
            {
                testVm.ReceivedParameter.Should().Be(expectedParameter,
                    "viewmodel should have received the expected parameter");
            }
        }
    }

    /// <summary>
    /// Builders for creating test scenarios.
    /// </summary>
    public class NavigationScenarioBuilder
    {
        private readonly IHostManager _hostManager;
        private readonly List<Action> _actions = new List<Action>();

        public NavigationScenarioBuilder(IHostManager hostManager)
        {
            _hostManager = hostManager ?? throw new ArgumentNullException(nameof(hostManager));
        }

        public NavigationScenarioBuilder RegisterHost(string hostName)
        {
            _actions.Add(() =>
            {
                var host = new NavigationHost();
                _hostManager.RegisterHost(hostName, host);
            });
            return this;
        }

        public NavigationScenarioBuilder Navigate<TView>(string hostName, object? parameter = null)
            where TView : Control
        {
            _actions.Add(() =>
            {
                _hostManager.Navigate<TView>(hostName, parameter);
            });
            return this;
        }

        public NavigationScenarioBuilder Delay(TimeSpan delay)
        {
            _actions.Add(() => Thread.Sleep(delay));
            return this;
        }

        public void Execute()
        {
            foreach (var action in _actions)
            {
                action();
            }
        }

        public async Task ExecuteAsync()
        {
            foreach (var action in _actions)
            {
                await Task.Run(action);
            }
        }
    }

    /// <summary>
    /// Test data generators.
    /// </summary>
    public static class TestDataGenerators
    {
        private static readonly Random _random = new Random();

        public static string GenerateRandomHostName() =>
            $"Host_{Guid.NewGuid():N}";

        public static string GenerateRandomString(int length = 10) =>
            new string(Enumerable.Range(0, length)
                .Select(_ => (char)_random.Next('a', 'z' + 1))
                .ToArray());

        public static IEnumerable<string> GenerateHostNames(int count) =>
            Enumerable.Range(0, count).Select(_ => GenerateRandomHostName());

        public static object GenerateComplexParameter() => new
        {
            Id = Guid.NewGuid(),
            Name = GenerateRandomString(),
            Timestamp = DateTime.UtcNow,
            Tags = Enumerable.Range(0, 5).Select(_ => GenerateRandomString(5)).ToArray()
        };
    }
}
