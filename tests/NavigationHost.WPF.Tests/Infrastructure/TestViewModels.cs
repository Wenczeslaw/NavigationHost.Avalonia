using NavigationHost.WPF.Abstractions;

namespace NavigationHost.WPF.Tests.Infrastructure
{
    /// <summary>
    ///     Test view model for navigation testing.
    /// </summary>
    public class TestViewModel : INavigationAware
    {
        public bool CanNavigateToWasCalled { get; private set; }
        public bool OnNavigatedToWasCalled { get; private set; }
        public bool CanNavigateFromWasCalled { get; private set; }
        public bool OnNavigatedFromWasCalled { get; private set; }
        public object? ReceivedParameter { get; private set; }

        public bool AllowNavigateTo { get; set; } = true;
        public bool AllowNavigateFrom { get; set; } = true;

        public bool CanNavigateTo(object? parameter)
        {
            CanNavigateToWasCalled = true;
            ReceivedParameter = parameter;
            return AllowNavigateTo;
        }

        public void OnNavigatedTo(object? parameter)
        {
            OnNavigatedToWasCalled = true;
            ReceivedParameter = parameter;
        }

        public bool CanNavigateFrom()
        {
            CanNavigateFromWasCalled = true;
            return AllowNavigateFrom;
        }

        public void OnNavigatedFrom()
        {
            OnNavigatedFromWasCalled = true;
        }

        public void Reset()
        {
            CanNavigateToWasCalled = false;
            OnNavigatedToWasCalled = false;
            CanNavigateFromWasCalled = false;
            OnNavigatedFromWasCalled = false;
            ReceivedParameter = null;
        }
    }

    /// <summary>
    ///     Another test view model.
    /// </summary>
    public class AnotherTestViewModel
    {
        public string Name { get; set; } = "Another Test ViewModel";
    }
}

