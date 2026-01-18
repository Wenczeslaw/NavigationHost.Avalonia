# NavigationHost.Avalonia

中文 | [English](#navigationhostavalonia-english)

一个轻量级且灵活的 Avalonia 应用程序导航库，灵感来自 Prism 的 RegionManager 模式。该库提供了一种简洁的方式来管理视图之间的导航，支持依赖注入、视图-视图模型映射以及多导航宿主。

## ✨ 特性

- 🎯 **多导航宿主** - 在应用程序中管理多个导航区域
- 🔄 **视图-视图模型映射** - 自动视图-视图模型关联和解析
- 💉 **依赖注入** - 完全支持 Microsoft.Extensions.DependencyInjection
- 📦 **导航感知** - INavigationAware 接口用于视图生命周期钩子
- 🎨 **XAML 优先设计** - 通过 XAML 附加属性轻松集成
- ⚡ **轻量级** - 最小依赖，支持 .NET Standard 2.0

## 🚀 快速开始

### 1. 安装包

```bash
dotnet add package NavigationHost.Avalonia
```

或通过 NuGet 包管理器：
```
Install-Package NavigationHost.Avalonia
```

### 2. 引入样式资源

**重要提示：** 在使用 NavigationHost 之前，需要在 `App.axaml` 中引入控件的样式资源：

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="YourApp.App">
    <Application.Styles>
        <FluentTheme />
        <!-- 引入 NavigationHost 样式 -->
        <StyleInclude Source="avares://NavigationHost.Avalonia/Themes/Generic.axaml" />
    </Application.Styles>
</Application>
```

### 3. 注册服务

**使用 DI 容器（推荐）：**

```csharp
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
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
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            desktop.MainWindow = serviceProvider.GetRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // 注册 NavigationHost 服务
        services.AddNavigationHost();

        // 注册视图和视图模型
        services.AddTransient<HomeView>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<SettingsView>();
        services.AddTransient<SettingsViewModel>();
        
        // 注册主窗口
        services.AddTransient<MainWindow>();
        services.AddTransient<MainWindowViewModel>();
    }
}
```

**约定式解析：**

库会根据命名约定自动解析视图模型：
- `HomeView` → `HomeViewModel`
- `SettingsView` → `SettingsViewModel`
- `MainWindow` → `MainWindowViewModel`

命名约定使用固定后缀："View" 用于视图，"ViewModel" 用于视图模型。

### 4. 在 XAML 中添加 NavigationHost

```xml
<Window x:Class="MyApp.MainWindow"
        xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:nav="http://schemas.navigationhost/avalonia"
        Title="我的应用" Width="800" Height="600">
    <Grid>
        <nav:NavigationHost nav:HostManager.HostName="MainRegion">
            <nav:NavigationHost.DefaultContent>
                <!-- 可选：默认显示的内容 -->
                <TextBlock Text="欢迎！请选择一个视图。" 
                          HorizontalAlignment="Center" 
                          VerticalAlignment="Center"/>
            </nav:NavigationHost.DefaultContent>
        </nav:NavigationHost>
    </Grid>
</Window>
```

### 5. 视图间导航

```csharp
public class MainWindowViewModel
{
    private readonly IHostManager _hostManager;
    
    public MainWindowViewModel(IHostManager hostManager)
    {
        _hostManager = hostManager;
    }
    
    // 使用泛型方法导航
    public void NavigateToHome()
    {
        _hostManager.Navigate<HomeView>("MainRegion");
    }
    
    // 使用类型导航
    public void NavigateByType()
    {
        _hostManager.Navigate("MainRegion", typeof(HomeView));
    }
}
```

## 📦 带参数的导航

```csharp
// 导航时传递参数
_hostManager.Navigate<DetailView>("MainRegion", parameter: userId);

// 实现 INavigationAware 接口以接收参数并控制导航
public class DetailViewModel : INavigationAware
{
    public bool CanNavigateTo(object? parameter)
    {
        // 在导航前调用，确认是否应该继续导航
        // 返回 true 允许导航，返回 false 取消导航
        return parameter is int;
    }
    
    public void OnNavigatedTo(object? parameter)
    {
        // 导航到此视图时调用
        if (parameter is int userId)
        {
            LoadUserData(userId);
        }
    }
    
    public bool CanNavigateFrom()
    {
        // 离开前调用，确认是否可以离开
        // 返回 true 允许离开，返回 false 停留在当前视图
        // 适用于确认未保存的更改
        if (HasUnsavedChanges)
        {
            // 在 Avalonia 中使用对话框
            var result = await ShowConfirmDialog(
                "您有未保存的更改，确定要离开吗？");
            return result;
        }
        return true;
    }
    
    public void OnNavigatedFrom()
    {
        // 离开此视图时调用
        // 执行清理操作
        CleanupResources();
    }
}
```

## 🎯 多导航宿主

您可以在应用程序中拥有多个独立的导航区域：

```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="200"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    
    <!-- 侧边栏导航 -->
    <nav:NavigationHost Grid.Column="0" 
                        nav:HostManager.HostName="SidebarRegion">
        <nav:NavigationHost.DefaultContent>
            <TextBlock Text="侧边栏" HorizontalAlignment="Center"/>
        </nav:NavigationHost.DefaultContent>
    </nav:NavigationHost>
    
    <!-- 主内容区 -->
    <nav:NavigationHost Grid.Column="1" 
                        nav:HostManager.HostName="ContentRegion">
        <nav:NavigationHost.DefaultContent>
            <TextBlock Text="主内容区" HorizontalAlignment="Center"/>
        </nav:NavigationHost.DefaultContent>
    </nav:NavigationHost>
</Grid>
```

在代码中导航到不同的区域：

```csharp
// 导航到侧边栏
_hostManager.Navigate<MenuView>("SidebarRegion");

// 导航到主内容区
_hostManager.Navigate<HomeView>("ContentRegion");

// 同时导航到多个区域
_hostManager.Navigate<MenuView>("SidebarRegion");
_hostManager.Navigate<DashboardView>("ContentRegion");
```

## 💡 高级用法

### 视图生命周期管理

```csharp
public class MyViewModel : INavigationAware
{
    private IDisposable? _subscription;
    
    public void OnNavigatedTo(object? parameter)
    {
        // 订阅事件或启动服务
        _subscription = SomeService.Subscribe(OnDataChanged);
    }
    
    public void OnNavigatedFrom()
    {
        // 取消订阅或释放资源
        _subscription?.Dispose();
        _subscription = null;
    }
    
    public bool CanNavigateTo(object? parameter) => true;
    public bool CanNavigateFrom() => true;
}
```

### 程序化注册视图

```csharp
// 获取 HostManager 实例
var hostManager = serviceProvider.GetRequiredService<IHostManager>();

// 程序化注册宿主
var navigationHost = new NavigationHost();
hostManager.RegisterHost("DynamicRegion", navigationHost);

// 导航到视图
hostManager.Navigate<MyView>("DynamicRegion");

// 取消注册宿主
hostManager.UnregisterHost("DynamicRegion");
```

### 获取已注册的宿主

```csharp
// 获取特定宿主
var host = hostManager.GetHost("MainRegion");

// 获取所有已注册的宿主名称
var hostNames = hostManager.GetHostNames();
foreach (var name in hostNames)
{
    Console.WriteLine($"已注册的宿主: {name}");
}
```

## 📋 完整示例

查看我们的示例项目以获取完整的工作示例：

```
NavigationHost.Sample.Avalonia/
├── ViewModels/
│   ├── MainWindowViewModel.cs
│   ├── HomeViewModel.cs
│   ├── ProductListViewModel.cs
│   ├── ProductDetailViewModel.cs
│   ├── SettingsViewModel.cs
│   └── UserProfileViewModel.cs
└── Views/
    ├── MainWindow.axaml
    ├── HomeView.axaml
    ├── ProductListView.axaml
    ├── ProductDetailView.axaml
    ├── SettingsView.axaml
    └── UserProfileView.axaml
```

---

# NavigationHost.Avalonia (English)

[中文](#navigationhostavalonia) | English

A lightweight and flexible navigation library for Avalonia applications, inspired by Prism's RegionManager pattern. This library provides a clean way to manage navigation between views with support for dependency injection, view-viewmodel mapping, and multiple navigation hosts.

## ✨ Features

- 🎯 **Multiple Navigation Hosts** - Manage multiple navigation regions in your application
- 🔄 **View-ViewModel Mapping** - Automatic view-viewmodel association and resolution
- 💉 **Dependency Injection** - Full support for Microsoft.Extensions.DependencyInjection
- 📦 **Navigation Awareness** - INavigationAware interface for view lifecycle hooks
- 🎨 **XAML-First Design** - Easy integration with XAML attached properties
- ⚡ **Lightweight** - Minimal dependencies, supports .NET Standard 2.0

## 🚀 Quick Start

### 1. Install Package

```bash
dotnet add package NavigationHost.Avalonia
```

Or via NuGet Package Manager:
```
Install-Package NavigationHost.Avalonia
```

### 2. Import Style Resources

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

### 3. Register Services

**Using DI Container (Recommended):**

```csharp
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
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
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            desktop.MainWindow = serviceProvider.GetRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register NavigationHost services
        services.AddNavigationHost();

        // Register views and viewmodels
        services.AddTransient<HomeView>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<SettingsView>();
        services.AddTransient<SettingsViewModel>();
        
        // Register main window
        services.AddTransient<MainWindow>();
        services.AddTransient<MainWindowViewModel>();
    }
}
```

**Convention-Based Resolution:**

The library automatically resolves ViewModels by naming convention:
- `HomeView` → `HomeViewModel`
- `SettingsView` → `SettingsViewModel`
- `MainWindow` → `MainWindowViewModel`

The naming convention uses fixed suffixes: "View" for views and "ViewModel" for view models.

### 4. Add NavigationHost to XAML

```xml
<Window x:Class="MyApp.MainWindow"
        xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:nav="http://schemas.navigationhost/avalonia"
        Title="My Application" Width="800" Height="600">
    <Grid>
        <nav:NavigationHost nav:HostManager.HostName="MainRegion">
            <nav:NavigationHost.DefaultContent>
                <!-- Optional: Default content to display -->
                <TextBlock Text="Welcome! Please select a view." 
                          HorizontalAlignment="Center" 
                          VerticalAlignment="Center"/>
            </nav:NavigationHost.DefaultContent>
        </nav:NavigationHost>
    </Grid>
</Window>
```

### 5. Navigate Between Views

```csharp
public class MainWindowViewModel
{
    private readonly IHostManager _hostManager;
    
    public MainWindowViewModel(IHostManager hostManager)
    {
        _hostManager = hostManager;
    }
    
    // Navigate using generic method
    public void NavigateToHome()
    {
        _hostManager.Navigate<HomeView>("MainRegion");
    }
    
    // Navigate by type
    public void NavigateByType()
    {
        _hostManager.Navigate("MainRegion", typeof(HomeView));
    }
}
```

## 📦 Navigation with Parameters

```csharp
// Pass parameters during navigation
_hostManager.Navigate<DetailView>("MainRegion", parameter: userId);

// Implement INavigationAware interface to receive parameters and control navigation
public class DetailViewModel : INavigationAware
{
    public bool CanNavigateTo(object? parameter)
    {
        // Called before navigation to confirm if navigation should proceed
        // Return true to allow navigation, false to cancel
        return parameter is int;
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
        // Called before leaving to confirm if leaving is allowed
        // Return true to allow leaving, false to stay on current view
        // Useful for confirming unsaved changes
        if (HasUnsavedChanges)
        {
            // Use dialog in Avalonia
            var result = await ShowConfirmDialog(
                "You have unsaved changes. Are you sure you want to leave?");
            return result;
        }
        return true;
    }
    
    public void OnNavigatedFrom()
    {
        // Called when leaving this view
        // Perform cleanup operations
        CleanupResources();
    }
}
```

## 🎯 Multiple Navigation Hosts

You can have multiple independent navigation regions in your application:

```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="200"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    
    <!-- Sidebar navigation -->
    <nav:NavigationHost Grid.Column="0" 
                        nav:HostManager.HostName="SidebarRegion">
        <nav:NavigationHost.DefaultContent>
            <TextBlock Text="Sidebar" HorizontalAlignment="Center"/>
        </nav:NavigationHost.DefaultContent>
    </nav:NavigationHost>
    
    <!-- Main content area -->
    <nav:NavigationHost Grid.Column="1" 
                        nav:HostManager.HostName="ContentRegion">
        <nav:NavigationHost.DefaultContent>
            <TextBlock Text="Main Content" HorizontalAlignment="Center"/>
        </nav:NavigationHost.DefaultContent>
    </nav:NavigationHost>
</Grid>
```

Navigate to different regions in code:

```csharp
// Navigate to sidebar
_hostManager.Navigate<MenuView>("SidebarRegion");

// Navigate to main content area
_hostManager.Navigate<HomeView>("ContentRegion");

// Navigate to multiple regions simultaneously
_hostManager.Navigate<MenuView>("SidebarRegion");
_hostManager.Navigate<DashboardView>("ContentRegion");
```

## 💡 Advanced Usage

### View Lifecycle Management

```csharp
public class MyViewModel : INavigationAware
{
    private IDisposable? _subscription;
    
    public void OnNavigatedTo(object? parameter)
    {
        // Subscribe to events or start services
        _subscription = SomeService.Subscribe(OnDataChanged);
    }
    
    public void OnNavigatedFrom()
    {
        // Unsubscribe or release resources
        _subscription?.Dispose();
        _subscription = null;
    }
    
    public bool CanNavigateTo(object? parameter) => true;
    public bool CanNavigateFrom() => true;
}
```

### Programmatic View Registration

```csharp
// Get HostManager instance
var hostManager = serviceProvider.GetRequiredService<IHostManager>();

// Programmatically register host
var navigationHost = new NavigationHost();
hostManager.RegisterHost("DynamicRegion", navigationHost);

// Navigate to view
hostManager.Navigate<MyView>("DynamicRegion");

// Unregister host
hostManager.UnregisterHost("DynamicRegion");
```

### Getting Registered Hosts

```csharp
// Get specific host
var host = hostManager.GetHost("MainRegion");

// Get all registered host names
var hostNames = hostManager.GetHostNames();
foreach (var name in hostNames)
{
    Console.WriteLine($"Registered host: {name}");
}
```

## 📋 Complete Example

Check out our sample project for a complete working example:

```
NavigationHost.Sample.Avalonia/
├── ViewModels/
│   ├── MainWindowViewModel.cs
│   ├── HomeViewModel.cs
│   ├── ProductListViewModel.cs
│   ├── ProductDetailViewModel.cs
│   ├── SettingsViewModel.cs
│   └── UserProfileViewModel.cs
└── Views/
    ├── MainWindow.axaml
    ├── HomeView.axaml
    ├── ProductListView.axaml
    ├── ProductDetailView.axaml
    ├── SettingsView.axaml
    └── UserProfileView.axaml
```

