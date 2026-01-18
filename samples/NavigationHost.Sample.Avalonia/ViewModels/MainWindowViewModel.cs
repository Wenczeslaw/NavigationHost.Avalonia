using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NavigationHost.Abstractions;
using NavigationHost.Sample.Avalonia.Views;

namespace NavigationHost.Sample.Avalonia.ViewModels;

public partial class MainWindowViewModel(IHostManager hostManager) : ViewModelBase
{
    private readonly IHostManager _hostManager = hostManager;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [RelayCommand]
    private void NavigateHome()
    {
        _hostManager.Navigate<HomeView>("MainHost");
    }

    [RelayCommand]
    private void NavigateProductList()
    {
        _hostManager.Navigate<ProductListView>("MainHost");
    }

    [RelayCommand]
    private void NavigateSettings()
    {
        _hostManager.Navigate<SettingsView>("MainHost");
    }

    [RelayCommand]
    private void NavigateUserProfile()
    {
        _hostManager.Navigate<UserProfileView>("SideHost", parameter: "User123");
    }
}

