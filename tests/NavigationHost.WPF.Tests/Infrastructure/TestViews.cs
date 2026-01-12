using System.Windows.Controls;

namespace NavigationHost.WPF.Tests.Infrastructure
{
    /// <summary>
    ///     Test view for navigation testing.
    /// </summary>
    public class TestView : UserControl
    {
        public TestView()
        {
            Content = new TextBlock { Text = "Test View" };
        }
    }

    /// <summary>
    ///     Another test view for navigation testing.
    /// </summary>
    public class AnotherTestView : UserControl
    {
        public AnotherTestView()
        {
            Content = new TextBlock { Text = "Another Test View" };
        }
    }

    /// <summary>
    ///     Test view with constructor parameter.
    /// </summary>
    public class TestViewWithParameter : UserControl
    {
        public object? Parameter { get; }

        public TestViewWithParameter(object? parameter = null)
        {
            Parameter = parameter;
            Content = new TextBlock { Text = $"Test View with Parameter: {parameter}" };
        }
    }
}

