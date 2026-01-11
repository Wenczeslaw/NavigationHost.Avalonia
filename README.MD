﻿﻿# NavigationHost.Avalonia

[中文](README.zh-CN.MD) | English

A lightweight and flexible navigation library for Avalonia applications, inspired by Prism's RegionManager pattern. This library provides a clean way to manage navigation between views with support for dependency injection, view-viewmodel mapping, and multiple navigation hosts.

## Features

- 🎯 **Multiple Navigation Hosts** - Manage multiple navigation regions in your application
- 🔄 **View-ViewModel Mapping** - Automatic view-viewmodel association and resolution with convention-based naming
- 💉 **Dependency Injection** - Full support for Microsoft.Extensions.DependencyInjection
- 📦 **Navigation Awareness** - INavigationAware interface for view lifecycle hooks
- 🎨 **XAML-First Design** - Easy integration with XAML attached properties
- ⚡ **Lightweight** - Minimal dependencies, supports .NET Standard 2.0 and .NET 8.0
- 🔍 **Convention-Based Resolution** - Automatically resolves ViewModels by naming convention (e.g., HomeView → HomeViewModel)

## Installation

```bash
# Install via NuGet
dotnet add package NavigationHost.Avalonia
```

Or add the package reference to your `.csproj` file:

```xml
<PackageReference Include="NavigationHost.Avalonia" Version="1.0.0" />
```

## Quick Start

### 1. Register Services

In your application startup (e.g., `Program.cs` or `App.axaml.cs`):

```csharp
using NavigationHost.Avalonia.Extensions;
using Microsoft.Extensions.DependencyInjection;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        
        // Register your views and viewmodels in DI
        // Convention-based resolution will automatically map them by naming
        services.AddTransient<HomeView>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<SettingsView>();
        services.AddTransient<SettingsViewModel>();
        
        // Add HostManager with convention-based resolution
        services.AddHostManager();
        
        // Register other services and viewmodels
        services.AddTransient<MainWindowViewModel>();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Create and show main window
        var mainWindow = new MainWindow
        {
            DataContext = serviceProvider.GetRequiredService<MainWindowViewModel>()
        };
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = mainWindow;
        }
        
        base.OnFrameworkInitializationCompleted();
    }
}
```

### 2. Add NavigationHost to XAML

In your main window or any view:

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:nav="http://schemas.navigationhost/avalonia"
        x:Class="YourApp.MainWindow">
    
    <Grid>
        <!-- Define a navigation host with a name -->
        <nav:NavigationHost nav:HostManager.HostName="MainRegion" />
    </Grid>
</Window>
```

### 3. Navigate Between Views

In your ViewModel or code-behind:

```csharp
using NavigationHost.Avalonia.Abstractions;

public class MainWindowViewModel
{
    private readonly IHostManager _hostManager;
    
    public MainWindowViewModel(IHostManager hostManager)
    {
        _hostManager = hostManager;
    }
    
    public void NavigateToHome()
    {
        // Navigate by view type
        _hostManager.Navigate("MainRegion", typeof(HomeView));
    }
    
    public void NavigateToSettings()
    {
        // Navigate using generic method
        _hostManager.Navigate<SettingsView>("MainRegion");
    }
}
```

## Core Concepts

### NavigationHost

`NavigationHost` is a templated control that displays the current view. You can have multiple navigation hosts in your application, each identified by a unique name.

```xml
<nav:NavigationHost nav:HostManager.HostName="MainRegion" 
                    DefaultContent="{Binding DefaultView}" />
```

### HostManager

`HostManager` is the central service that manages all navigation hosts and coordinates navigation operations. It acts as a facade for several specialized internal services.

**Key Features:**
- Register and manage navigation hosts
- Navigate between views
- Support for view-viewmodel mapping (explicit and convention-based)
- Integration with IoC containers
- Automatic ViewModel resolution by naming convention

### View-ViewModel Resolution

The library uses **convention-based resolution** to automatically resolve ViewModels by naming convention:

**Naming Conventions:**
1. If view name ends with "View" suffix: removes "View" and adds "ViewModel"
   - `HomeView` → `HomeViewModel`
   - `SettingsView` → `SettingsViewModel`

2. If view name doesn't end with "View": directly appends "ViewModel"
   - `Home` → `HomeViewModel`
   - `MainWindow` → `MainWindowViewModel`

The naming conventions use fixed suffixes: "View" for views and "ViewModel" for view models.

### INavigationAware Interface

Implement this interface in your ViewModels to receive navigation lifecycle notifications and control navigation flow:

```csharp
using NavigationHost.Avalonia.Abstractions;

public class MyViewModel : INavigationAware
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
        Console.WriteLine($"Navigated with parameter: {parameter}");
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
        Console.WriteLine("Navigating away");
    }
}
```

## Advanced Usage

### Navigation with Parameters

```csharp
// Pass parameters during navigation
_hostManager.Navigate("MainRegion", typeof(DetailView), parameter: userId);

// Receive in INavigationAware
public void OnNavigatedTo(object? parameter)
{
    if (parameter is int userId)
    {
        LoadUserData(userId);
    }
}
```

### Multiple Navigation Hosts

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:nav="http://schemas.navigationhost/avalonia"
        x:Class="YourApp.MainWindow">
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
</Window>
```

```csharp
// Navigate in different regions
_hostManager.Navigate<MenuView>("SidebarRegion");
_hostManager.Navigate<HomeView>("ContentRegion");
```

### Direct Navigation from View

```csharp
public partial class MyView : UserControl
{
    private INavigationService? NavigationService =>
        (this.Parent as NavigationHost.Avalonia.NavigationHost);
        
    public void SomeMethod()
    {
        // Direct navigation to another view
        NavigationService?.Navigate(new AnotherView());
    }
}
```

## API Reference

### IHostManager

Main interface for navigation management:

```csharp
public interface IHostManager
{
    void RegisterHost(string hostName, NavigationHost host);
    bool UnregisterHost(string hostName);
    NavigationHost? GetHost(string hostName);
    IEnumerable<string> GetHostNames();
    void Navigate(string hostName, Control content);
    void Navigate(string hostName, Type contentType, object? parameter = null);
    void Navigate<T>(string hostName, object? parameter = null) where T : Control;
}
```

### INavigationHost

Interface implemented by NavigationHost:

```csharp
public interface INavigationHost
{
    Control? CurrentContent { get; }
    event EventHandler<NavigationEventArgs>? Navigated;
    void Navigate(Control content);
}
```

### INavigationAware

Interface for ViewModels that need navigation lifecycle hooks:

```csharp
public interface INavigationAware
{
    bool CanNavigateTo(object? parameter);
    void OnNavigatedTo(object? parameter);
    bool CanNavigateFrom();
    void OnNavigatedFrom();
}
```

## Architecture

The library follows a clean architecture with clear separation of concerns:

- **NavigationHost** - UI control that displays views and implements INavigationHost
- **HostManager** - Facade service coordinating all navigation operations
- **Abstractions** - Interfaces defining contracts
  - `IHostManager` - Main service interface
  - `INavigationHost` - Basic navigation interface
  - `INavigationAware` - Navigation lifecycle interface
  - `IHostRegistry` - Host registration management
  - `IViewModelConventionResolver` - Convention-based resolution
- **Internal Services**:
  - `HostRegistry` - Manages navigation host registration
  - `ViewModelConventionResolver` - Convention-based view-viewmodel resolution
  - `InstanceFactory` - Creates instances via DI container
- **Extensions** - DI integration helpers
- **Events** - Navigation event arguments
- **Options** - Configuration options for convention-based resolution

## Requirements

- .NET Standard 2.0 or .NET 8.0
- Avalonia 11.3.10 or higher
- Microsoft.Extensions.DependencyInjection.Abstractions 8.0.0 or higher (for DI support)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Inspired by Prism's RegionManager pattern
- Built for the Avalonia UI framework

## Support

If you encounter any issues or have questions, please file an issue on the GitHub repository.

