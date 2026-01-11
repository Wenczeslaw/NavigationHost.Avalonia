﻿# NavigationHost.Avalonia

中文 | [English](README.MD)

一个轻量级且灵活的 Avalonia 应用程序导航库，灵感来自 Prism 的 RegionManager 模式。该库提供了一种清晰的方式来管理视图之间的导航，支持依赖注入、视图-视图模型映射和多个导航主机。

## 特性

- 🎯 **多导航主机** - 在应用程序中管理多个导航区域
- 🔄 **视图-视图模型映射** - 自动关联和解析视图与视图模型，支持基于约定的命名
- 💉 **依赖注入** - 完全支持 Microsoft.Extensions.DependencyInjection
- 📦 **导航感知** - INavigationAware 接口用于视图生命周期钩子
- 🎨 **XAML 优先设计** - 通过 XAML 附加属性轻松集成
- ⚡ **轻量级** - 最少的依赖项，支持 .NET Standard 2.0 和 .NET 8.0
- 🔍 **基于约定的解析** - 通过命名约定自动解析 ViewModel（例如 HomeView → HomeViewModel）

## 安装

```bash
# 通过 NuGet 安装
dotnet add package NavigationHost.Avalonia
```

或在 `.csproj` 文件中添加包引用：

```xml
<PackageReference Include="NavigationHost.Avalonia" Version="1.0.0" />
```

## 快速开始

### 1. 注册服务

在应用程序启动时（例如 `Program.cs` 或 `App.axaml.cs`）：

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
        
        // 在 DI 中注册视图和视图模型
        // 基于约定的解析会根据命名自动映射它们
        services.AddTransient<HomeView>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<SettingsView>();
        services.AddTransient<SettingsViewModel>();
        
        // 添加具有基于约定解析功能的 HostManager
        services.AddHostManager();
        
        // 注册其他服务和视图模型
        services.AddTransient<MainWindowViewModel>();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // 创建并显示主窗口
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

### 2. 在 XAML 中添加 NavigationHost

在主窗口或任何视图中：

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:nav="http://schemas.navigationhost/avalonia"
        x:Class="YourApp.MainWindow">
    
    <Grid>
        <!-- 定义一个带名称的导航主机 -->
        <nav:NavigationHost nav:HostManager.HostName="MainRegion" />
    </Grid>
</Window>
```

### 3. 在视图之间导航

在视图模型或代码隐藏中：

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
        // 通过视图类型导航
        _hostManager.Navigate("MainRegion", typeof(HomeView));
    }
    
    public void NavigateToSettings()
    {
        // 使用泛型方法导航
        _hostManager.Navigate<SettingsView>("MainRegion");
    }
}
```

## 核心概念

### NavigationHost（导航主机）

`NavigationHost` 是一个模板化控件，用于显示当前视图。您可以在应用程序中拥有多个导航主机，每个主机由唯一的名称标识。

```xml
<nav:NavigationHost nav:HostManager.HostName="MainRegion" 
                    DefaultContent="{Binding DefaultView}" />
```

### HostManager（主机管理器）

`HostManager` 是管理所有导航主机并协调导航操作的中心服务。它作为多个专业内部服务的门面。

**主要功能：**
- 注册和管理导航主机
- 在视图之间导航
- 支持视图-视图模型映射（显式和基于约定）
- 与 IoC 容器集成
- 通过命名约定自动解析 ViewModel

### 视图-视图模型解析

该库使用**基于约定的解析**通过命名约定自动解析 ViewModel：

**命名约定：**
1. 如果视图名称以 "View" 后缀结尾：移除 "View" 并添加 "ViewModel"
   - `HomeView` → `HomeViewModel`
   - `SettingsView` → `SettingsViewModel`

2. 如果视图名称不以 "View" 结尾：直接附加 "ViewModel"
   - `Home` → `HomeViewModel`
   - `MainWindow` → `MainWindowViewModel`

命名约定使用固定的后缀："View" 用于视图，"ViewModel" 用于视图模型。

### INavigationAware 接口

在视图模型中实现此接口以接收导航生命周期通知并控制导航流程：

```csharp
using NavigationHost.Avalonia.Abstractions;

public class MyViewModel : INavigationAware
{
    public bool CanNavigateTo(object? parameter)
    {
        // 在导航前调用，确认是否应该继续导航
        // 返回 true 允许导航，false 取消导航
        return true;
    }
    
    public void OnNavigatedTo(object? parameter)
    {
        // 导航到此视图时调用
        Console.WriteLine($"导航参数：{parameter}");
    }
    
    public bool CanNavigateFrom()
    {
        // 在离开前调用，确认是否应该允许离开
        // 返回 true 允许离开，false 停留在当前视图
        // 对于确认未保存的更改很有用
        return true;
    }
    
    public void OnNavigatedFrom()
    {
        // 从此视图导航离开时调用
        Console.WriteLine("正在离开");
    }
}
```

## 高级用法

### 带参数的导航

```csharp
// 在导航时传递参数
_hostManager.Navigate("MainRegion", typeof(DetailView), parameter: userId);

// 在 INavigationAware 中接收
public void OnNavigatedTo(object? parameter)
{
    if (parameter is int userId)
    {
        LoadUserData(userId);
    }
}
```

### 多个导航主机

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
        
        <!-- 侧边栏导航 -->
        <nav:NavigationHost Grid.Column="0" 
                            nav:HostManager.HostName="SidebarRegion" />
        
        <!-- 主内容 -->
        <nav:NavigationHost Grid.Column="1" 
                            nav:HostManager.HostName="ContentRegion" />
    </Grid>
</Window>
```

```csharp
// 在不同区域中导航
_hostManager.Navigate<MenuView>("SidebarRegion");
_hostManager.Navigate<HomeView>("ContentRegion");
```

### 从视图直接导航

```csharp
public partial class MyView : UserControl
{
    private INavigationService? NavigationService =>
        (this.Parent as NavigationHost.Avalonia.NavigationHost);
        
    public void SomeMethod()
    {
        // 直接导航到另一个视图
        NavigationService?.Navigate(new AnotherView());
    }
}
```

## API 参考

### IHostManager

导航管理的主接口：

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

由 NavigationHost 实现的接口：

```csharp
public interface INavigationHost
{
    Control? CurrentContent { get; }
    event EventHandler<NavigationEventArgs>? Navigated;
    void Navigate(Control content);
}
```

### INavigationAware

用于需要导航生命周期钩子的 ViewModel 的接口：

```csharp
public interface INavigationAware
{
    bool CanNavigateTo(object? parameter);
    void OnNavigatedTo(object? parameter);
    bool CanNavigateFrom();
    void OnNavigatedFrom();
}
```

## 架构

该库遵循清晰的架构，具有明确的关注点分离：

- **NavigationHost** - 显示视图并实现 INavigationHost 的 UI 控件
- **HostManager** - 协调所有导航操作的门面服务
- **Abstractions（抽象）** - 定义契约的接口
  - `IHostManager` - 主服务接口
  - `INavigationHost` - 基本导航接口
  - `INavigationAware` - 导航生命周期接口
  - `IHostRegistry` - 主机注册管理
  - `IViewModelConventionResolver` - 基于约定的解析
- **内部服务**：
  - `HostRegistry` - 管理导航主机注册
  - `ViewModelConventionResolver` - 基于约定的视图-视图模型解析
  - `InstanceFactory` - 通过 DI 容器创建实例
- **Extensions（扩展）** - DI 集成助手
- **Events（事件）** - 导航事件参数
- **Options（选项）** - 基于约定解析的配置选项

## 要求

- .NET Standard 2.0 或 .NET 8.0
- Avalonia 11.3.10 或更高版本
- Microsoft.Extensions.DependencyInjection.Abstractions 8.0.0 或更高版本（用于 DI 支持）

## 贡献

欢迎贡献！请随时提交 Pull Request。

## 许可证

该项目采用 MIT 许可证 - 详情请参阅 [LICENSE](LICENSE) 文件。

## 致谢

- 灵感来自 Prism 的 RegionManager 模式
- 为 Avalonia UI 框架构建

## 支持

如果您遇到任何问题或有疑问，请在 GitHub 仓库中提交 issue。

