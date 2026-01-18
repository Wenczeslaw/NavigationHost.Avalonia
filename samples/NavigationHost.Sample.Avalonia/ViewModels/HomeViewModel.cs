using CommunityToolkit.Mvvm.ComponentModel;

namespace NavigationHost.Sample.Avalonia.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "Home";

    [ObservableProperty]
    private string _description = "This is the Home view demonstrating basic navigation.";

    [ObservableProperty]
    private string _features = @"✓ Basic navigation without parameters
✓ Simple view display
✓ No navigation lifecycle hooks";

    public HomeViewModel()
    {
        // Simple view without INavigationAware
    }
}

