﻿# Avalonia.Navigation

A lightweight and flexible navigation library for Avalonia applications, inspired by Prism's RegionManager pattern. This library provides a clean way to manage navigation between views with support for dependency injection, view-viewmodel mapping, and multiple navigation hosts.

## Features

- 🎯 **Multiple Navigation Hosts** - Manage multiple navigation regions in your application
- 🔄 **View-ViewModel Mapping** - Automatic view-viewmodel association and resolution
- 💉 **Dependency Injection** - Full support for Microsoft.Extensions.DependencyInjection
- 📦 **Navigation Awareness** - INavigationAware interface for view lifecycle hooks
- 🎨 **XAML-First Design** - Easy integration with XAML attached properties
- ⚡ **Lightweight** - Minimal dependencies, targets .NET Standard 2.0

## Quick Start

### 1. Register Services

**Using DI Container (Recommended):**

```csharp
using NavigationHost.Avalonia.Extensions;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Register views and viewmodels in DI
// Convention-based resolution will automatically map them by naming
services.AddTransient<HomeView>();
services.AddTransient<HomeViewModel>();
services.AddTransient<SettingsView>();
services.AddTransient<SettingsViewModel>();

// Add HostManager with convention-based resolution
services.AddHostManager();

var serviceProvider = services.BuildServiceProvider();
```

**Convention-Based Resolution:**

The library automatically resolves ViewModels by naming convention:
- `HomeView` → `HomeViewModel`
- `SettingsView` → `SettingsViewModel`
- `MainWindow` → `MainWindowViewModel`

The naming conventions use fixed suffixes: "View" for views and "ViewModel" for view models.

### 2. Add NavigationHost to XAML

```xml
<Window xmlns:nav="using:NavigationHost.Avalonia">
    <nav:NavigationHost nav:HostManager.HostName="MainRegion" />
</Window>
```

### 3. Navigate Between Views

```csharp
public class MainViewModel
{
    private readonly IHostManager _hostManager;
    
    public MainViewModel(IHostManager hostManager)
    {
        _hostManager = hostManager;
    }
    
    public void NavigateToHome()
    {
        _hostManager.Navigate<HomeView>("MainRegion");
    }
    
    public void NavigateByType()
    {
        _hostManager.Navigate("MainRegion", typeof(HomeView));
    }
}
```

## Navigation with Parameters

```csharp
// Pass parameters during navigation
_hostManager.Navigate("MainRegion", typeof(DetailView), parameter: userId);

// Implement INavigationAware to receive parameters and control navigation
public class DetailViewModel : INavigationAware
{
    public bool CanNavigateTo(object? parameter)
    {
        // Called before navigation to confirm if navigation should proceed
        // Return true to allow navigation, false to cancel
        return true;
    }
    
    public void OnNavigatedTo(object? parameter)
    {
        // Called when navigating to this view
        if (parameter is int userId)
        {
            LoadUserData(userId);
        }
    }
    
    public bool CanNavigateFrom()
    {
        // Called before navigating away to confirm if leaving should proceed
        // Return true to allow leaving, false to stay on current view
        // Useful for confirming unsaved changes
        return true;
    }
    
    public void OnNavigatedFrom()
    {
        // Called when navigating away from this view
        // Cleanup when navigating away
    }
}
```

## Multiple Navigation Hosts

```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="200"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    
    <!-- Sidebar navigation -->
    <nav:NavigationHost Grid.Column="0" 
                        nav:HostManager.HostName="SidebarRegion" />
    
    <!-- Main content -->
    <nav:NavigationHost Grid.Column="1" 
                        nav:HostManager.HostName="ContentRegion" />
</Grid>
```

## Requirements

- .NET Standard 2.0 or higher
- Avalonia 11.3.10 or higher
- Microsoft.Extensions.DependencyInjection.Abstractions 8.0.0 or higher (for DI support)

## Documentation

For complete documentation and advanced usage, please visit:
- [GitHub Repository](https://github.com/Wenczeslaw/Avalonia.Navigation)
- [Full README with Examples](https://github.com/Wenczeslaw/Avalonia.Navigation/blob/main/README.MD)
- [中文文档](https://github.com/Wenczeslaw/Avalonia.Navigation/blob/main/README.zh-CN.MD)

## License

This project is licensed under the MIT License.

## Support

If you encounter any issues or have questions, please file an issue on the [GitHub repository](https://github.com/Wenczeslaw/Avalonia.Navigation/issues).

