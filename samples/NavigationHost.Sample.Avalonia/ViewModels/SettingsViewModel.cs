using CommunityToolkit.Mvvm.ComponentModel;
using NavigationHost.Abstractions;

namespace NavigationHost.Sample.Avalonia.ViewModels;

/// <summary>
/// Demonstrates CanNavigateFrom with confirmation
/// </summary>
public partial class SettingsViewModel : ViewModelBase, INavigationAware
{
    [ObservableProperty]
    private string _title = "Settings";

    [ObservableProperty]
    private string _userName = "John Doe";

    [ObservableProperty]
    private string _email = "john@example.com";

    [ObservableProperty]
    private bool _notificationsEnabled = true;

    [ObservableProperty]
    private bool _isDirty = false;

    [ObservableProperty]
    private string _navigationInfo = "Make some changes to test navigation confirmation";

    partial void OnUserNameChanged(string value) => IsDirty = true;
    partial void OnEmailChanged(string value) => IsDirty = true;
    partial void OnNotificationsEnabledChanged(bool value) => IsDirty = true;

    // INavigationAware implementation
    public bool CanNavigateTo(object? parameter)
    {
        return true;
    }

    public void OnNavigatedTo(object? parameter)
    {
        IsDirty = false;
        NavigationInfo = "Make some changes to test navigation confirmation (CanNavigateFrom)";
    }

    public bool CanNavigateFrom()
    {
        // This demonstrates navigation confirmation
        // In a real app, you would show a dialog here
        if (IsDirty)
        {
            NavigationInfo = "⚠️ Navigation blocked! Changes detected. (In real app, show confirmation dialog)";
            // Simulate user choosing to stay
            return false; // Block navigation
        }
        return true;
    }

    public void OnNavigatedFrom()
    {
        IsDirty = false;
    }
}

