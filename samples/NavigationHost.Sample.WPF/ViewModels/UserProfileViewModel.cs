using CommunityToolkit.Mvvm.ComponentModel;
using NavigationHost.Abstractions;

namespace NavigationHost.Sample.WPF.ViewModels;

/// <summary>
/// Demonstrates navigation in secondary host with parameters
/// </summary>
public partial class UserProfileViewModel : ViewModelBase, INavigationAware
{
    [ObservableProperty]
    private string _title = "User Profile";

    [ObservableProperty]
    private string _userId = string.Empty;

    [ObservableProperty]
    private string _userName = "Loading...";

    [ObservableProperty]
    private string _memberSince = string.Empty;

    [ObservableProperty]
    private string _navigationInfo = string.Empty;

    // INavigationAware implementation
    public bool CanNavigateTo(object? parameter)
    {
        return true;
    }

    public void OnNavigatedTo(object? parameter)
    {
        if (parameter is string userId)
        {
            UserId = userId;
            LoadUserProfile(userId);
            NavigationInfo = $"Loaded user profile for: {userId}";
        }
        else
        {
            NavigationInfo = "No user ID parameter provided";
        }
    }

    private void LoadUserProfile(string userId)
    {
        // Simulate loading user data
        UserId = userId;
        UserName = $"User {userId}";
        MemberSince = "January 2024";
        Title = $"Profile - {UserName}";
    }

    public bool CanNavigateFrom()
    {
        return true;
    }

    public void OnNavigatedFrom()
    {
        // Cleanup
    }
}
