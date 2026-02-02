# NavigationHost.Avalonia

[中文](README.zh-CN.MD) | English

A lightweight and flexible navigation library for Avalonia applications, inspired by Prism's RegionManager pattern. This library provides a clean way to manage navigation between views with support for dependency injection, view-viewmodel mapping, and multiple navigation hosts.

## ✨ Features

- 🎯 **Multiple Navigation Hosts** - Manage multiple navigation regions in your application
- 🔄 **View-ViewModel Mapping** - Automatic view-viewmodel association and resolution with convention-based naming
- 💉 **Dependency Injection** - Full support for Microsoft.Extensions.DependencyInjection
- 📦 **Navigation Awareness** - INavigationAware interface for view lifecycle hooks
- 🎨 **XAML-First Design** - Easy integration with XAML attached properties
- ⚡ **Lightweight** - Minimal dependencies, supports .NET Standard 2.0 and .NET 8.0
- 🔍 **Convention-Based Resolution** - Automatically resolves ViewModels by naming convention (e.g., HomeView → HomeViewModel)

## 🚀 Installation

```bash
# Install via NuGet
dotnet add package NavigationHost.Avalonia
```

Or via NuGet Package Manager:
```
Install-Package NavigationHost.Avalonia
```

Or add the package reference to your `.csproj` file:

```xml
<PackageReference Include="NavigationHost.Avalonia" Version="1.0.0" />
```

## Quick Start

### 1. Import Style Resources

**Important:** Before using NavigationHost, you need to include the control's style resources in your `App.axaml`:

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="YourApp.App">
    <Application.Styles>
        <FluentTheme />
        <!-- Import NavigationHost styles -->
        <StyleInclude Source="avares://NavigationHost.Avalonia/Themes/Generic.axaml" />
    </Application.Styles>
</Application>
```

### 2. Register Services

Configure dependency injection at application startup (`App.axaml.cs`):

```csharp
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NavigationHost.Avalonia.Extensions;
using Microsoft.Extensions.DependencyInjection;

public class App : Application
{
    public IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Configure dependency injection
            var services = new ServiceCollection();
            ConfigureServices(services);
            Services = services.BuildServiceProvider();

            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register NavigationHost services
        services.AddHostManager();

        // Register ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<ProductListViewModel>();
        services.AddTransient<ProductDetailViewModel>();

        // Register Views (optional, for DI-based view creation)
        services.AddTransient<HomeView>();
        services.AddTransient<SettingsView>();
        services.AddTransient<ProductListView>();
        services.AddTransient<ProductDetailView>();
    }
}
```

**Convention-Based Resolution:**

The library automatically resolves ViewModels by naming convention:
- `HomeView` → `HomeViewModel`
- `SettingsView` → `SettingsViewModel`
- `MainWindow` → `MainWindowViewModel`

The naming convention uses fixed suffixes: "View" for views and "ViewModel" for view models.

### 3. Add NavigationHost to XAML

In your main window or any view (`MainWindow.axaml`):

**Complete Example:**

```xml
<Window x:Class="YourApp.MainWindow"
        xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:nav="http://schemas.navigationhost/avalonia"
        xmlns:vm="using:YourApp.ViewModels"
        Title="My Application" Width="1200" Height="700"
        x:DataType="vm:MainWindowViewModel"
        Icon="/Assets/app-icon.ico">
    
    <Grid>
        <!-- Define a named navigation host -->
        <nav:NavigationHost nav:HostManager.HostName="MainRegion">
            <nav:NavigationHost.DefaultContent>
                <!-- Optional: Default content to display -->
                <TextBlock Text="Welcome! Please select a view." 
                          HorizontalAlignment="Center" 
                          VerticalAlignment="Center"
                          FontSize="18"/>
            </nav:NavigationHost.DefaultContent>
        </nav:NavigationHost>
    </Grid>
</Window>
```

### 4. Navigate Between Views

In your ViewModel:

```csharp
using NavigationHost.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IHostManager _hostManager;
    
    public MainWindowViewModel(IHostManager hostManager)
    {
        _hostManager = hostManager;
    }
    
    [RelayCommand]
    private void NavigateToHome()
    {
        // Navigate using generic method
        _hostManager.Navigate<HomeView>("MainRegion");
    }
    
    [RelayCommand]
    private void NavigateToSettings()
    {
        // Navigate by type
        _hostManager.Navigate("MainRegion", typeof(SettingsView));
    }
}
```

## Core Concepts

### NavigationHost

`NavigationHost` is a templated control that displays the current view. You can have multiple navigation hosts in your application, each identified by a unique name.

```xml
<nav:NavigationHost nav:HostManager.HostName="MainRegion">
    <nav:NavigationHost.DefaultContent>
        <!-- Default content displayed when no view is navigated to -->
        <StackPanel HorizontalAlignment="Center" 
                   VerticalAlignment="Center" 
                   Spacing="10">
            <TextBlock Text="📍 Welcome to the Navigation System" 
                      FontSize="24" 
                      FontWeight="Bold"/>
            <TextBlock Text="Please select a view from the menu" 
                      FontSize="16" 
                      Opacity="0.7"/>
        </StackPanel>
    </nav:NavigationHost.DefaultContent>
</nav:NavigationHost>
```

### HostManager

`HostManager` is the central service that manages all navigation hosts and coordinates navigation operations. It acts as a facade for several specialized internal services.

**Key Features:**
- ✅ Register and manage navigation hosts
- ✅ Navigate between views
- ✅ Support for view-viewmodel mapping (explicit and convention-based)
- ✅ Integration with IoC containers
- ✅ Automatic ViewModel resolution by naming convention
- ✅ Pass navigation parameters
- ✅ Control navigation flow

### View-ViewModel Resolution

The library uses **convention-based resolution** to automatically resolve ViewModels by naming convention:

**Naming Conventions:**
1. If the view name ends with "View" suffix: removes "View" and adds "ViewModel"
   - `HomeView` → `HomeViewModel`
   - `SettingsView` → `SettingsViewModel`
   - `ProductDetailView` → `ProductDetailViewModel`

2. If the view name doesn't end with "View": directly appends "ViewModel"
   - `Home` → `HomeViewModel`
   - `MainWindow` → `MainWindowViewModel`

The naming convention uses fixed suffixes: "View" for views and "ViewModel" for view models.

**Example:**
```csharp
// Automatically resolves HomeViewModel and sets it as DataContext
_hostManager.Navigate<HomeView>("MainRegion");
```

### INavigationAware Interface

Implement this interface in your ViewModels to receive navigation lifecycle notifications and control navigation flow:

```csharp
using NavigationHost.Abstractions;

public class MyViewModel : INavigationAware
{
    public bool CanNavigateTo(object? parameter)
    {
        // Called before navigation to confirm if navigation should proceed
        // Return true to allow navigation, false to cancel
        // Example: Check user permissions
        return HasPermission();
    }
    
    public void OnNavigatedTo(object? parameter)
    {
        // Called when navigating to this view
        // Receive navigation parameters and initialize view
        if (parameter is int userId)
        {
            LoadUserData(userId);
        }
    }
    
    public bool CanNavigateFrom()
    {
        // Called before leaving to confirm if leaving should be allowed
        // Return true to allow leaving, false to stay on current view
        // Useful for confirming unsaved changes
        if (HasUnsavedChanges)
        {
            // Show confirmation dialog
            return ShowUnsavedChangesDialog();
        }
        return true;
    }
    
    public void OnNavigatedFrom()
    {
        // Called when navigating away from this view
        // Perform cleanup operations
        CleanupResources();
        UnsubscribeEvents();
    }
}
```

## 📦 Navigation with Parameters

You can pass parameters to the target ViewModel during navigation:

```csharp
// Pass a single parameter
_hostManager.Navigate<ProductDetailView>("MainRegion", parameter: productId);

// Pass a complex object
var orderData = new OrderInfo { Id = 123, CustomerId = 456 };
_hostManager.Navigate<OrderDetailView>("MainRegion", parameter: orderData);

// Receive parameters in ViewModel
public class ProductDetailViewModel : INavigationAware
{
    private int _productId;
    
    public bool CanNavigateTo(object? parameter)
    {
        // Validate if parameter is valid
        return parameter is int;
    }
    
    public void OnNavigatedTo(object? parameter)
    {
        if (parameter is int productId)
        {
            _productId = productId;
            LoadProductDetails(productId);
        }
    }
    
    private async void LoadProductDetails(int productId)
    {
        // Load product details
        var product = await _productService.GetProductByIdAsync(productId);
        // Update UI
    }
    
    public bool CanNavigateFrom() => true;
    public void OnNavigatedFrom() { }
}
```

## 🎯 Multiple Navigation Hosts

You can have multiple independent navigation regions in your application:

```xml
<Grid RowDefinitions="Auto,*" ColumnDefinitions="200,*">
    <!-- Top toolbar -->
    <Border Grid.Row="0" Grid.ColumnSpan="2" 
            Background="#6366F1" 
            Padding="20">
        <TextBlock Text="My Application" 
                  FontSize="24" 
                  FontWeight="Bold" 
                  Foreground="White"/>
    </Border>
    
    <!-- Sidebar navigation -->
    <nav:NavigationHost Grid.Row="1" Grid.Column="0" 
                        nav:HostManager.HostName="SidebarRegion">
        <nav:NavigationHost.DefaultContent>
            <TextBlock Text="Sidebar Menu" 
                      HorizontalAlignment="Center" 
                      VerticalAlignment="Top"
                      Margin="0,20,0,0"/>
        </nav:NavigationHost.DefaultContent>
    </nav:NavigationHost>
    
    <!-- Main content area -->
    <nav:NavigationHost Grid.Row="1" Grid.Column="1" 
                        nav:HostManager.HostName="ContentRegion">
        <nav:NavigationHost.DefaultContent>
            <StackPanel HorizontalAlignment="Center" 
                       VerticalAlignment="Center">
                <TextBlock Text="Welcome!" 
                          FontSize="32" 
                          FontWeight="Bold"/>
                <TextBlock Text="Please select a view from the sidebar" 
                          FontSize="16" 
                          Opacity="0.7"
                          Margin="0,10,0,0"/>
            </StackPanel>
        </nav:NavigationHost.DefaultContent>
    </nav:NavigationHost>
</Grid>
```

Navigate to different regions in code:

```csharp
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IHostManager _hostManager;
    
    public MainWindowViewModel(IHostManager hostManager)
    {
        _hostManager = hostManager;
    }
    
    [RelayCommand]
    private void ShowDashboard()
    {
        // Navigate both regions simultaneously
        _hostManager.Navigate<MenuView>("SidebarRegion");
        _hostManager.Navigate<DashboardView>("ContentRegion");
    }
    
    [RelayCommand]
    private void ShowProductList()
    {
        _hostManager.Navigate<ProductListView>("ContentRegion");
    }
    
    [RelayCommand]
    private void ShowProductDetail(int productId)
    {
        _hostManager.Navigate<ProductDetailView>("ContentRegion", parameter: productId);
    }
}
```

## 💡 Advanced Usage

### View Lifecycle Management

Complete lifecycle management example:

```csharp
public class ProductListViewModel : ObservableObject, INavigationAware
{
    private readonly IProductService _productService;
    private IDisposable? _updateSubscription;
    private CancellationTokenSource? _cts;
    
    public ObservableCollection<Product> Products { get; } = new();
    
    public ProductListViewModel(IProductService productService)
    {
        _productService = productService;
    }
    
    public bool CanNavigateTo(object? parameter)
    {
        // Check if navigation to this view is allowed
        // Example: Check user permissions
        return true;
    }
    
    public async void OnNavigatedTo(object? parameter)
    {
        // Create cancellation token
        _cts = new CancellationTokenSource();
        
        // Subscribe to real-time updates
        _updateSubscription = _productService
            .ProductUpdates
            .Subscribe(OnProductUpdated);
        
        // Load data
        await LoadProductsAsync(_cts.Token);
    }
    
    public bool CanNavigateFrom()
    {
        // Check for unsaved changes
        if (HasUnsavedChanges)
        {
            // Show dialog in Avalonia
            var result = ShowConfirmationDialog(
                "You have unsaved changes. Are you sure you want to leave?",
                "Confirm");
            return result;
        }
        return true;
    }
    
    public void OnNavigatedFrom()
    {
        // Cancel async operations
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        
        // Unsubscribe
        _updateSubscription?.Dispose();
        _updateSubscription = null;
        
        // Cleanup resources
        Products.Clear();
    }
    
    private async Task LoadProductsAsync(CancellationToken ct)
    {
        var products = await _productService.GetProductsAsync(ct);
        foreach (var product in products)
        {
            Products.Add(product);
        }
    }
    
    private void OnProductUpdated(Product product)
    {
        // Handle product updates
        var existing = Products.FirstOrDefault(p => p.Id == product.Id);
        if (existing != null)
        {
            var index = Products.IndexOf(existing);
            Products[index] = product;
        }
    }
}
```

### Programmatic View Registration

```csharp
public class DynamicRegionManager
{
    private readonly IHostManager _hostManager;
    
    public DynamicRegionManager(IHostManager hostManager)
    {
        _hostManager = hostManager;
    }
    
    public void CreateDynamicRegion(string regionName)
    {
        // Create new navigation host
        var navigationHost = new NavigationHost();
        
        // Register with HostManager
        _hostManager.RegisterHost(regionName, navigationHost);
        
        // Navigate to initial view
        _hostManager.Navigate<HomeView>(regionName);
    }
    
    public void RemoveDynamicRegion(string regionName)
    {
        // Unregister host
        var removed = _hostManager.UnregisterHost(regionName);
        
        if (removed)
        {
            Console.WriteLine($"Region '{regionName}' has been removed");
        }
    }
    
    public void ListAllRegions()
    {
        // Get all registered host names
        var hostNames = _hostManager.GetHostNames();
        
        Console.WriteLine("Registered navigation regions:");
        foreach (var name in hostNames)
        {
            Console.WriteLine($"  - {name}");
        }
    }
}
```

### Getting Registered Hosts

```csharp
public class NavigationInspector
{
    private readonly IHostManager _hostManager;
    
    public NavigationInspector(IHostManager hostManager)
    {
        _hostManager = hostManager;
    }
    
    public void InspectRegions()
    {
        // Get specific host
        var mainHost = _hostManager.GetHost("MainRegion");
        if (mainHost != null)
        {
            Console.WriteLine($"MainRegion current content: {mainHost.CurrentContent?.GetType().Name}");
        }
        
        // Get all registered host names
        var hostNames = _hostManager.GetHostNames();
        Console.WriteLine($"\nTotal {hostNames.Count()} registered regions:");
        
        foreach (var name in hostNames)
        {
            var host = _hostManager.GetHost(name);
            var contentType = host?.CurrentContent?.GetType().Name ?? "No content";
            Console.WriteLine($"  📍 {name}: {contentType}");
        }
    }
}
```

### Inter-View Communication

Use the Messenger pattern to pass messages between different views:

```csharp
// Define message
public record ProductSelectedMessage(int ProductId);

// Send message (in ProductListViewModel)
public partial class ProductListViewModel : ObservableObject
{
    private readonly IMessenger _messenger;
    
    [RelayCommand]
    private void SelectProduct(Product product)
    {
        _messenger.Send(new ProductSelectedMessage(product.Id));
    }
}

// Receive message (in ProductDetailViewModel)
public partial class ProductDetailViewModel : ObservableObject, 
    INavigationAware, 
    IRecipient<ProductSelectedMessage>
{
    private readonly IMessenger _messenger;
    
    public ProductDetailViewModel(IMessenger messenger)
    {
        _messenger = messenger;
    }
    
    public void OnNavigatedTo(object? parameter)
    {
        // Register to receive messages
        _messenger.Register(this);
        
        if (parameter is int productId)
        {
            LoadProduct(productId);
        }
    }
    
    public void OnNavigatedFrom()
    {
        // Unregister
        _messenger.Unregister<ProductSelectedMessage>(this);
    }
    
    public void Receive(ProductSelectedMessage message)
    {
        LoadProduct(message.ProductId);
    }
}
```

## 📋 API Reference

### IHostManager

Main interface for navigation management:

```csharp
public interface IHostManager
{
    /// <summary>
    /// Register a navigation host
    /// </summary>
    /// <param name="hostName">Unique name of the host</param>
    /// <param name="host">NavigationHost instance</param>
    void RegisterHost(string hostName, NavigationHost host);
    
    /// <summary>
    /// Unregister a navigation host
    /// </summary>
    /// <param name="hostName">Name of the host to unregister</param>
    /// <returns>True if successfully unregistered, false otherwise</returns>
    bool UnregisterHost(string hostName);
    
    /// <summary>
    /// Get the navigation host with the specified name
    /// </summary>
    /// <param name="hostName">Host name</param>
    /// <returns>NavigationHost instance, or null if not found</returns>
    NavigationHost? GetHost(string hostName);
    
    /// <summary>
    /// Get all registered host names
    /// </summary>
    /// <returns>Collection of host names</returns>
    IEnumerable<string> GetHostNames();
    
    /// <summary>
    /// Check if a host with the specified name exists
    /// </summary>
    /// <param name="hostName">Name of the host to check</param>
    /// <returns>True if the host exists, false otherwise</returns>
    bool HostExists(string hostName);
    
    /// <summary>
    /// Navigate to a specific control instance
    /// </summary>
    /// <param name="hostName">Target host name</param>
    /// <param name="content">Control to display</param>
    void Navigate(string hostName, Control content);
    
    /// <summary>
    /// Navigate to a view of the specified type
    /// </summary>
    /// <param name="hostName">Target host name</param>
    /// <param name="contentType">View type</param>
    /// <param name="parameter">Optional navigation parameter</param>
    void Navigate(string hostName, Type contentType, object? parameter = null);
    
    /// <summary>
    /// Navigate to a view of the specified generic type
    /// </summary>
    /// <typeparam name="T">View type</typeparam>
    /// <param name="hostName">Target host name</param>
    /// <param name="parameter">Optional navigation parameter</param>
    void Navigate<T>(string hostName, object? parameter = null) where T : Control;
}
```

### INavigationHost

Interface implemented by NavigationHost:

```csharp
public interface INavigationHost
{
    /// <summary>
    /// Get the currently displayed content
    /// </summary>
    Control? CurrentContent { get; }
    
    /// <summary>
    /// Event fired after navigation completes
    /// </summary>
    event EventHandler<NavigationEventArgs>? Navigated;
    
    /// <summary>
    /// Navigate to the specified control
    /// </summary>
    /// <param name="content">Control to display</param>
    void Navigate(Control content);
}
```

### INavigationAware

Interface for ViewModels that need navigation lifecycle hooks:

```csharp
public interface INavigationAware
{
    /// <summary>
    /// Called before navigating to the view
    /// </summary>
    /// <param name="parameter">Navigation parameter</param>
    /// <returns>True to allow navigation, false otherwise</returns>
    bool CanNavigateTo(object? parameter);
    
    /// <summary>
    /// Called when navigating to the view
    /// </summary>
    /// <param name="parameter">Navigation parameter</param>
    void OnNavigatedTo(object? parameter);
    
    /// <summary>
    /// Called before leaving the view
    /// </summary>
    /// <returns>True to allow leaving, false otherwise</returns>
    bool CanNavigateFrom();
    
    /// <summary>
    /// Called when leaving the view
    /// </summary>
    void OnNavigatedFrom();
}
```

### IHostRegistry

Host registration management interface:

```csharp
public interface IHostRegistry
{
    /// <summary>
    /// Register a navigation host
    /// </summary>
    void Register(string hostName, INavigationHost host);
    
    /// <summary>
    /// Unregister a navigation host
    /// </summary>
    bool Unregister(string hostName);
    
    /// <summary>
    /// Get the specified navigation host
    /// </summary>
    INavigationHost? Get(string hostName);
    
    /// <summary>
    /// Get all host names
    /// </summary>
    IEnumerable<string> GetHostNames();
}
```

### IViewModelConventionResolver

Convention-based view-viewmodel resolution interface:

```csharp
public interface IViewModelConventionResolver
{
    /// <summary>
    /// Resolve the ViewModel type for a view
    /// </summary>
    /// <param name="viewType">View type</param>
    /// <returns>ViewModel type, or null if unable to resolve</returns>
    Type? ResolveViewModelType(Type viewType);
}
```

### RequestNavigate Extension Methods (Prism-style)

Extension methods for safe navigation with automatic retry when host is not ready:

```csharp
using NavigationHost.Avalonia.Extensions;  // or NavigationHost.WPF.Extensions

public static class HostManagerNavigationExtensions
{
    /// <summary>
    /// Attempts to navigate with optional retry if host is not ready.
    /// Similar to Prism's RequestNavigate pattern.
    /// </summary>
    void RequestNavigate(
        this IHostManager hostManager,
        string hostName,
        Type contentType,
        object? parameter = null,
        Action<NavigationResult>? onComplete = null,
        bool retryOnHostNotReady = true);
    
    /// <summary>
    /// Generic version of RequestNavigate
    /// </summary>
    void RequestNavigate<T>(
        this IHostManager hostManager,
        string hostName,
        object? parameter = null,
        Action<NavigationResult>? onComplete = null,
        bool retryOnHostNotReady = true);
    
    /// <summary>
    /// Async version of RequestNavigate
    /// </summary>
    Task<NavigationResult> RequestNavigateAsync(
        this IHostManager hostManager,
        string hostName,
        Type contentType,
        object? parameter = null,
        bool retryOnHostNotReady = true);
    
    /// <summary>
    /// Generic async version of RequestNavigate
    /// </summary>
    Task<NavigationResult> RequestNavigateAsync<T>(
        this IHostManager hostManager,
        string hostName,
        object? parameter = null,
        bool retryOnHostNotReady = true);
}
```

**NavigationResult:**

```csharp
public class NavigationResult
{
    public bool Success { get; set; }
    public Exception? Error { get; set; }
}
```

**Usage Example:**

```csharp
using NavigationHost.Avalonia.Extensions;

public partial class NavigationViewModel : ObservableObject
{
    private readonly IHostManager _hostManager;
    private readonly ILogger _logger;
    
    partial void OnCurrentMenuItemChanged(MenuItem? value)
    {
        if (value == null) return;
        
        // Use RequestNavigate for automatic retry when host is not ready
        _hostManager.RequestNavigate(
            "MainHost",
            value.ViewType,
            parameter: null,
            onComplete: result =>
            {
                if (!result.Success)
                {
                    _logger.LogError(result.Error, "Navigation failed");
                }
            },
            retryOnHostNotReady: true  // Automatically retry if host not ready
        );
    }
    
    // Or use async version
    private async Task NavigateAsync(Type viewType)
    {
        var result = await _hostManager.RequestNavigateAsync(
            "MainHost",
            viewType,
            retryOnHostNotReady: true
        );
        
        if (!result.Success)
        {
            ShowError(result.Error?.Message);
        }
    }
}
```

**Benefits:**
- ✅ Automatically handles host not ready situations
- ✅ Provides navigation result callbacks
- ✅ Similar API to Prism's RequestNavigate
- ✅ Supports both sync and async patterns


## 🏗️ Architecture

The library follows a clean architecture with clear separation of concerns:

### Layer Structure

```
┌─────────────────────────────────────────┐
│          Application Layer               │
│  (Views, ViewModels, Services)          │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│      NavigationHost.Avalonia            │
│  ┌─────────────────────────────────┐   │
│  │   HostManager (Facade)          │   │
│  │   - Unified Navigation API      │   │
│  └──────────┬──────────────────────┘   │
│             │                            │
│  ┌──────────▼──────────────────────┐   │
│  │   Internal Services             │   │
│  │  - HostRegistry                 │   │
│  │  - ViewModelConventionResolver  │   │
│  │  - InstanceFactory              │   │
│  └─────────────────────────────────┘   │
│                                          │
│  ┌─────────────────────────────────┐   │
│  │   UI Controls                   │   │
│  │  - NavigationHost               │   │
│  └─────────────────────────────────┘   │
└──────────────────────────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│    NavigationHost.Abstractions          │
│  (Interfaces & Contracts)               │
└─────────────────────────────────────────┘
```

### Component Description

- **NavigationHost** - UI control that displays views and implements INavigationHost
- **HostManager** - Facade service coordinating all navigation operations
- **Abstractions** - Interfaces defining contracts
  - `IHostManager` - Main service interface
  - `INavigationHost` - Basic navigation interface
  - `INavigationAware` - Navigation lifecycle interface
  - `IHostRegistry` - Host registration management
  - `IViewModelConventionResolver` - Convention-based resolution
  - `IContentAdapter` - Content adapter interface
  - `IInstanceFactory` - Instance factory interface
- **Internal Services**:
  - `HostRegistry` - Manages navigation host registration
  - `ViewModelConventionResolver` - Convention-based view-viewmodel resolution
  - `InstanceFactory` - Creates instances via DI container
- **Extensions** - DI integration helpers
  - `ServiceCollectionExtensions` - Service registration extensions
- **Adapters** - Platform-specific adapters
- **Themes** - Default styles and templates

## 📋 Complete Example

Check out our sample project for a complete working example:

### Project Structure

```
NavigationHost.Sample.Avalonia/
├── App.axaml                          # Application XAML
├── App.axaml.cs                       # Application startup and DI configuration
├── Program.cs                         # Program entry point
├── ViewLocator.cs                     # View locator (optional)
│
├── ViewModels/                        # ViewModels
│   ├── ViewModelBase.cs               # Base class
│   ├── MainWindowViewModel.cs         # Main window ViewModel
│   ├── HomeViewModel.cs               # Home ViewModel
│   ├── ProductListViewModel.cs        # Product list ViewModel
│   ├── ProductDetailViewModel.cs      # Product detail ViewModel
│   ├── SettingsViewModel.cs           # Settings ViewModel
│   └── UserProfileViewModel.cs        # User profile ViewModel
│
└── Views/                             # Views
    ├── MainWindow.axaml               # Main window
    ├── HomeView.axaml                 # Home view
    ├── ProductListView.axaml          # Product list view
    ├── ProductDetailView.axaml        # Product detail view
    ├── SettingsView.axaml             # Settings view
    └── UserProfileView.axaml          # User profile view
```

### Run the Sample

```bash
cd samples/NavigationHost.Sample.Avalonia
dotnet run
```

The sample application demonstrates:
- ✅ Using multiple navigation hosts
- ✅ Navigation between views
- ✅ Navigation with parameters
- ✅ INavigationAware lifecycle hooks
- ✅ Dependency injection integration
- ✅ MVVM pattern best practices

## 🔧 System Requirements

- **.NET Standard 2.0** or higher
- **.NET 6.0**, **.NET 8.0** or higher
- **Avalonia 11.3.10** or higher
- **Microsoft.Extensions.DependencyInjection.Abstractions 8.0.0** or higher (for DI support)

### Compatibility

| Platform | Support |
|----------|---------|
| Windows | ✅ |
| Linux | ✅ |
| macOS | ✅ |
| Browser (WASM) | ✅ |
| iOS | ✅ |
| Android | ✅ |

## 🎓 Best Practices

### 1. Use Dependency Injection

Always register views and viewmodels through the DI container instead of manually creating instances:

```csharp
// ✅ Recommended
services.AddTransient<HomeView>();
services.AddTransient<HomeViewModel>();

// ❌ Not recommended
var view = new HomeView();
var viewModel = new HomeViewModel();
```

### 2. Follow Naming Conventions

Use consistent naming conventions to leverage automatic resolution:

```csharp
// ✅ Recommended
HomeView.axaml → HomeViewModel.cs
ProductListView.axaml → ProductListViewModel.cs

// ❌ Not recommended
Home.axaml → HomeVm.cs
ProductList.axaml → ProductViewModel.cs
```

### 3. Implement INavigationAware

For ViewModels that need to be navigation-aware, implement the INavigationAware interface:

```csharp
// ✅ Recommended
public class MyViewModel : ObservableObject, INavigationAware
{
    public void OnNavigatedTo(object? parameter) { /* Initialize */ }
    public void OnNavigatedFrom() { /* Cleanup */ }
    public bool CanNavigateTo(object? parameter) => true;
    public bool CanNavigateFrom() => true;
}
```

### 4. Resource Cleanup

Always cleanup resources in OnNavigatedFrom:

```csharp
public void OnNavigatedFrom()
{
    // Unsubscribe from events
    _subscription?.Dispose();
    
    // Cancel async operations
    _cts?.Cancel();
    _cts?.Dispose();
    
    // Clear collections
    Items.Clear();
}
```

### 5. Parameter Validation

Validate parameters in CanNavigateTo:

```csharp
public bool CanNavigateTo(object? parameter)
{
    // Validate parameter type and validity
    if (parameter is not int id || id <= 0)
    {
        return false;
    }
    return true;
}
```

## 🤝 Contributing

Contributions of all kinds are welcome!

### How to Contribute

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Contribution Guidelines

- Follow existing code style
- Add unit tests for new features
- Update relevant documentation
- Ensure all tests pass

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

```
MIT License

Copyright (c) 2024 NavigationHost.Avalonia

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## 🙏 Acknowledgments

- Inspired by [Prism](https://prismlibrary.com/)'s RegionManager pattern
- Thanks to all contributors for their hard work

## 📚 Related Resources

- [Avalonia Official Documentation](https://docs.avaloniaui.net/)
- [MVVM Pattern Guide](https://docs.avaloniaui.net/docs/concepts/the-mvvm-pattern/)
- [Dependency Injection Best Practices](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)

## 💬 Support

If you encounter any issues or have questions, please seek help through:

- 📝 [Submit an Issue](https://github.com/Wenczeslaw/Avalonia.Navigation/issues)
- 💬 [Discussions](https://github.com/Wenczeslaw/Avalonia.Navigation/discussions)
- 📧 Contact maintainers via GitHub

## 🗺️ Roadmap

<div align="center">

**If this project helps you, please give us a ⭐️ Star!**

[Report Bug](https://github.com/Wenczeslaw/Avalonia.Navigation/issues) · [Request Feature](https://github.com/Wenczeslaw/Avalonia.Navigation/issues) · [Documentation](https://github.com/Wenczeslaw/Avalonia.Navigation)

Made with ❤️ by the NavigationHost.Avalonia team

</div>
