# NavigationHost.Avalonia

中文 | [English](README.MD)

一个轻量级且灵活的 Avalonia 应用程序导航库，灵感来自 Prism 的 RegionManager 模式。该库提供了一种简洁的方式来管理视图之间的导航，支持依赖注入、视图-视图模型映射以及多导航宿主。

## ✨ 特性

- 🎯 **多导航宿主** - 在应用程序中管理多个导航区域
- 🔄 **视图-视图模型映射** - 自动视图-视图模型关联和解析，支持基于约定的命名
- 💉 **依赖注入** - 完全支持 Microsoft.Extensions.DependencyInjection
- 📦 **导航感知** - INavigationAware 接口用于视图生命周期钩子
- 🎨 **XAML 优先设计** - 通过 XAML 附加属性轻松集成
- ⚡ **轻量级** - 最小依赖，支持 .NET Standard 2.0 和 .NET 8.0
- 🔍 **基于约定的解析** - 通过命名约定自动解析视图模型（例如 HomeView → HomeViewModel）

## 🚀 安装

```bash
# 通过 NuGet 安装
dotnet add package NavigationHost.Avalonia
```

或通过 NuGet 包管理器：
```
Install-Package NavigationHost.Avalonia
```

或在 `.csproj` 文件中添加包引用：

```xml
<PackageReference Include="NavigationHost.Avalonia" Version="1.0.0" />
```

## 快速开始

### 1. 引入样式资源

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

### 2. 注册服务

在应用程序启动时（`App.axaml.cs`）配置依赖注入：

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
            // 配置依赖注入
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
        // 注册 NavigationHost 服务
        services.AddHostManager();

        // 注册视图模型
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<ProductListViewModel>();
        services.AddTransient<ProductDetailViewModel>();

        // 注册视图（可选，用于基于 DI 的视图创建）
        services.AddTransient<HomeView>();
        services.AddTransient<SettingsView>();
        services.AddTransient<ProductListView>();
        services.AddTransient<ProductDetailView>();
    }
}
```

**约定式解析：**

库会根据命名约定自动解析视图模型：
- `HomeView` → `HomeViewModel`
- `SettingsView` → `SettingsViewModel`
- `MainWindow` → `MainWindowViewModel`

命名约定使用固定后缀："View" 用于视图，"ViewModel" 用于视图模型。

### 3. 在 XAML 中添加 NavigationHost

在主窗口或任何视图中（`MainWindow.axaml`）：

**完整示例：**

```xml
<Window x:Class="YourApp.MainWindow"
        xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:nav="http://schemas.navigationhost/avalonia"
        xmlns:vm="using:YourApp.ViewModels"
        Title="我的应用" Width="1200" Height="700"
        x:DataType="vm:MainWindowViewModel"
        Icon="/Assets/app-icon.ico">
    
    <Grid>
        <!-- 定义一个带名称的导航宿主 -->
        <nav:NavigationHost nav:HostManager.HostName="MainRegion">
            <nav:NavigationHost.DefaultContent>
                <!-- 可选：默认显示的内容 -->
                <TextBlock Text="欢迎！请选择一个视图。" 
                          HorizontalAlignment="Center" 
                          VerticalAlignment="Center"
                          FontSize="18"/>
            </nav:NavigationHost.DefaultContent>
        </nav:NavigationHost>
    </Grid>
</Window>
```

### 4. 视图间导航

在视图模型中：

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
        // 使用泛型方法导航
        _hostManager.Navigate<HomeView>("MainRegion");
    }
    
    [RelayCommand]
    private void NavigateToSettings()
    {
        // 使用类型导航
        _hostManager.Navigate("MainRegion", typeof(SettingsView));
    }
}
```

## 核心概念

### NavigationHost（导航宿主）

`NavigationHost` 是一个模板化控件，用于显示当前视图。您可以在应用程序中拥有多个导航宿主，每个宿主由唯一的名称标识。

```xml
<nav:NavigationHost nav:HostManager.HostName="MainRegion">
    <nav:NavigationHost.DefaultContent>
        <!-- 未导航到任何视图时显示的默认内容 -->
        <StackPanel HorizontalAlignment="Center" 
                   VerticalAlignment="Center" 
                   Spacing="10">
            <TextBlock Text="📍 欢迎使用导航系统" 
                      FontSize="24" 
                      FontWeight="Bold"/>
            <TextBlock Text="请从菜单中选择一个视图" 
                      FontSize="16" 
                      Opacity="0.7"/>
        </StackPanel>
    </nav:NavigationHost.DefaultContent>
</nav:NavigationHost>
```

### HostManager（主机管理器）

`HostManager` 是管理所有导航宿主并协调导航操作的中心服务。它作为多个专业内部服务的门面。

**主要功能：**
- ✅ 注册和管理导航宿主
- ✅ 在视图之间导航
- ✅ 支持视图-视图模型映射（显式和基于约定）
- ✅ 与 IoC 容器集成
- ✅ 通过命名约定自动解析视图模型
- ✅ 传递导航参数
- ✅ 控制导航流程

### 视图-视图模型解析

该库使用**基于约定的解析**通过命名约定自动解析视图模型：

**命名约定：**
1. 如果视图名称以 "View" 后缀结尾：移除 "View" 并添加 "ViewModel"
   - `HomeView` → `HomeViewModel`
   - `SettingsView` → `SettingsViewModel`
   - `ProductDetailView` → `ProductDetailViewModel`

2. 如果视图名称不以 "View" 结尾：直接附加 "ViewModel"
   - `Home` → `HomeViewModel`
   - `MainWindow` → `MainWindowViewModel`

命名约定使用固定的后缀："View" 用于视图，"ViewModel" 用于视图模型。

**示例：**
```csharp
// 自动解析 HomeViewModel 并设置为 DataContext
_hostManager.Navigate<HomeView>("MainRegion");
```

### INavigationAware 接口

在视图模型中实现此接口以接收导航生命周期通知并控制导航流程：

```csharp
using NavigationHost.Abstractions;

public class MyViewModel : INavigationAware
{
    public bool CanNavigateTo(object? parameter)
    {
        // 在导航前调用，确认是否应该继续导航
        // 返回 true 允许导航，false 取消导航
        // 例如：检查用户权限
        return HasPermission();
    }
    
    public void OnNavigatedTo(object? parameter)
    {
        // 导航到此视图时调用
        // 接收导航参数并初始化视图
        if (parameter is int userId)
        {
            LoadUserData(userId);
        }
    }
    
    public bool CanNavigateFrom()
    {
        // 在离开前调用，确认是否应该允许离开
        // 返回 true 允许离开，false 停留在当前视图
        // 适用于确认未保存的更改
        if (HasUnsavedChanges)
        {
            // 显示确认对话框
            return ShowUnsavedChangesDialog();
        }
        return true;
    }
    
    public void OnNavigatedFrom()
    {
        // 从此视图导航离开时调用
        // 执行清理操作
        CleanupResources();
        UnsubscribeEvents();
    }
}
```

## 📦 带参数的导航

导航时可以传递参数给目标视图模型：

```csharp
// 传递单个参数
_hostManager.Navigate<ProductDetailView>("MainRegion", parameter: productId);

// 传递复杂对象
var orderData = new OrderInfo { Id = 123, CustomerId = 456 };
_hostManager.Navigate<OrderDetailView>("MainRegion", parameter: orderData);

// 在视图模型中接收参数
public class ProductDetailViewModel : INavigationAware
{
    private int _productId;
    
    public bool CanNavigateTo(object? parameter)
    {
        // 验证参数是否有效
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
        // 加载产品详情
        var product = await _productService.GetProductByIdAsync(productId);
        // 更新 UI
    }
    
    public bool CanNavigateFrom() => true;
    public void OnNavigatedFrom() { }
}
```

## 🎯 多导航宿主

您可以在应用程序中拥有多个独立的导航区域：

```xml
<Grid RowDefinitions="Auto,*" ColumnDefinitions="200,*">
    <!-- 顶部工具栏 -->
    <Border Grid.Row="0" Grid.ColumnSpan="2" 
            Background="#6366F1" 
            Padding="20">
        <TextBlock Text="我的应用程序" 
                  FontSize="24" 
                  FontWeight="Bold" 
                  Foreground="White"/>
    </Border>
    
    <!-- 侧边栏导航 -->
    <nav:NavigationHost Grid.Row="1" Grid.Column="0" 
                        nav:HostManager.HostName="SidebarRegion">
        <nav:NavigationHost.DefaultContent>
            <TextBlock Text="侧边栏菜单" 
                      HorizontalAlignment="Center" 
                      VerticalAlignment="Top"
                      Margin="0,20,0,0"/>
        </nav:NavigationHost.DefaultContent>
    </nav:NavigationHost>
    
    <!-- 主内容区 -->
    <nav:NavigationHost Grid.Row="1" Grid.Column="1" 
                        nav:HostManager.HostName="ContentRegion">
        <nav:NavigationHost.DefaultContent>
            <StackPanel HorizontalAlignment="Center" 
                       VerticalAlignment="Center">
                <TextBlock Text="欢迎！" 
                          FontSize="32" 
                          FontWeight="Bold"/>
                <TextBlock Text="请从侧边栏选择一个视图" 
                          FontSize="16" 
                          Opacity="0.7"
                          Margin="0,10,0,0"/>
            </StackPanel>
        </nav:NavigationHost.DefaultContent>
    </nav:NavigationHost>
</Grid>
```

在代码中导航到不同的区域：

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
        // 同时导航两个区域
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

## 💡 高级用法

### 视图生命周期管理

完整的生命周期管理示例：

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
        // 检查是否允许导航到此视图
        // 例如：检查用户权限
        return true;
    }
    
    public async void OnNavigatedTo(object? parameter)
    {
        // 创建取消令牌
        _cts = new CancellationTokenSource();
        
        // 订阅实时更新
        _updateSubscription = _productService
            .ProductUpdates
            .Subscribe(OnProductUpdated);
        
        // 加载数据
        await LoadProductsAsync(_cts.Token);
    }
    
    public bool CanNavigateFrom()
    {
        // 检查是否有未保存的更改
        if (HasUnsavedChanges)
        {
            // 在 Avalonia 中显示对话框
            var result = ShowConfirmationDialog(
                "您有未保存的更改，确定要离开吗？",
                "确认");
            return result;
        }
        return true;
    }
    
    public void OnNavigatedFrom()
    {
        // 取消异步操作
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        
        // 取消订阅
        _updateSubscription?.Dispose();
        _updateSubscription = null;
        
        // 清理资源
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
        // 处理产品更新
        var existing = Products.FirstOrDefault(p => p.Id == product.Id);
        if (existing != null)
        {
            var index = Products.IndexOf(existing);
            Products[index] = product;
        }
    }
}
```

### 程序化注册视图

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
        // 创建新的导航宿主
        var navigationHost = new NavigationHost();
        
        // 注册到 HostManager
        _hostManager.RegisterHost(regionName, navigationHost);
        
        // 导航到初始视图
        _hostManager.Navigate<HomeView>(regionName);
    }
    
    public void RemoveDynamicRegion(string regionName)
    {
        // 取消注册宿主
        var removed = _hostManager.UnregisterHost(regionName);
        
        if (removed)
        {
            Console.WriteLine($"区域 '{regionName}' 已被移除");
        }
    }
    
    public void ListAllRegions()
    {
        // 获取所有已注册的宿主名称
        var hostNames = _hostManager.GetHostNames();
        
        Console.WriteLine("已注册的导航区域：");
        foreach (var name in hostNames)
        {
            Console.WriteLine($"  - {name}");
        }
    }
}
```

### 获取已注册的宿主

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
        // 获取特定宿主
        var mainHost = _hostManager.GetHost("MainRegion");
        if (mainHost != null)
        {
            Console.WriteLine($"MainRegion 当前内容: {mainHost.CurrentContent?.GetType().Name}");
        }
        
        // 获取所有已注册的宿主名称
        var hostNames = _hostManager.GetHostNames();
        Console.WriteLine($"\n共有 {hostNames.Count()} 个注册的区域：");
        
        foreach (var name in hostNames)
        {
            var host = _hostManager.GetHost(name);
            var contentType = host?.CurrentContent?.GetType().Name ?? "无内容";
            Console.WriteLine($"  📍 {name}: {contentType}");
        }
    }
}
```

### 视图间通信

使用 Messenger 模式在不同视图之间传递消息：

```csharp
// 定义消息
public record ProductSelectedMessage(int ProductId);

// 发送消息（在 ProductListViewModel 中）
public partial class ProductListViewModel : ObservableObject
{
    private readonly IMessenger _messenger;
    
    [RelayCommand]
    private void SelectProduct(Product product)
    {
        _messenger.Send(new ProductSelectedMessage(product.Id));
    }
}

// 接收消息（在 ProductDetailViewModel 中）
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
        // 注册接收消息
        _messenger.Register(this);
        
        if (parameter is int productId)
        {
            LoadProduct(productId);
        }
    }
    
    public void OnNavigatedFrom()
    {
        // 取消注册
        _messenger.Unregister<ProductSelectedMessage>(this);
    }
    
    public void Receive(ProductSelectedMessage message)
    {
        LoadProduct(message.ProductId);
    }
}
```

## 📋 API 参考

### IHostManager

导航管理的主接口：

```csharp
public interface IHostManager
{
    /// <summary>
    /// 注册一个导航宿主
    /// </summary>
    /// <param name="hostName">宿主的唯一名称</param>
    /// <param name="host">NavigationHost 实例</param>
    void RegisterHost(string hostName, NavigationHost host);
    
    /// <summary>
    /// 取消注册一个导航宿主
    /// </summary>
    /// <param name="hostName">要取消注册的宿主名称</param>
    /// <returns>如果成功取消注册返回 true，否则返回 false</returns>
    bool UnregisterHost(string hostName);
    
    /// <summary>
    /// 获取指定名称的导航宿主
    /// </summary>
    /// <param name="hostName">宿主名称</param>
    /// <returns>NavigationHost 实例，如果未找到则返回 null</returns>
    NavigationHost? GetHost(string hostName);
    
    /// <summary>
    /// 获取所有已注册的宿主名称
    /// </summary>
    /// <returns>宿主名称的集合</returns>
    IEnumerable<string> GetHostNames();
    
    /// <summary>
    /// 导航到指定的控件实例
    /// </summary>
    /// <param name="hostName">目标宿主名称</param>
    /// <param name="content">要显示的控件</param>
    void Navigate(string hostName, Control content);
    
    /// <summary>
    /// 导航到指定类型的视图
    /// </summary>
    /// <param name="hostName">目标宿主名称</param>
    /// <param name="contentType">视图类型</param>
    /// <param name="parameter">可选的导航参数</param>
    void Navigate(string hostName, Type contentType, object? parameter = null);
    
    /// <summary>
    /// 导航到指定泛型类型的视图
    /// </summary>
    /// <typeparam name="T">视图类型</typeparam>
    /// <param name="hostName">目标宿主名称</param>
    /// <param name="parameter">可选的导航参数</param>
    void Navigate<T>(string hostName, object? parameter = null) where T : Control;
}
```

### INavigationHost

由 NavigationHost 实现的接口：

```csharp
public interface INavigationHost
{
    /// <summary>
    /// 获取当前显示的内容
    /// </summary>
    Control? CurrentContent { get; }
    
    /// <summary>
    /// 导航完成后触发的事件
    /// </summary>
    event EventHandler<NavigationEventArgs>? Navigated;
    
    /// <summary>
    /// 导航到指定的控件
    /// </summary>
    /// <param name="content">要显示的控件</param>
    void Navigate(Control content);
}
```

### INavigationAware

用于需要导航生命周期钩子的视图模型的接口：

```csharp
public interface INavigationAware
{
    /// <summary>
    /// 在导航到视图之前调用
    /// </summary>
    /// <param name="parameter">导航参数</param>
    /// <returns>如果允许导航返回 true，否则返回 false</returns>
    bool CanNavigateTo(object? parameter);
    
    /// <summary>
    /// 导航到视图时调用
    /// </summary>
    /// <param name="parameter">导航参数</param>
    void OnNavigatedTo(object? parameter);
    
    /// <summary>
    /// 在离开视图之前调用
    /// </summary>
    /// <returns>如果允许离开返回 true，否则返回 false</returns>
    bool CanNavigateFrom();
    
    /// <summary>
    /// 离开视图时调用
    /// </summary>
    void OnNavigatedFrom();
}
```

### IHostRegistry

宿主注册管理接口：

```csharp
public interface IHostRegistry
{
    /// <summary>
    /// 注册导航宿主
    /// </summary>
    void Register(string hostName, INavigationHost host);
    
    /// <summary>
    /// 取消注册导航宿主
    /// </summary>
    bool Unregister(string hostName);
    
    /// <summary>
    /// 获取指定的导航宿主
    /// </summary>
    INavigationHost? Get(string hostName);
    
    /// <summary>
    /// 获取所有宿主名称
    /// </summary>
    IEnumerable<string> GetHostNames();
}
```

### IViewModelConventionResolver

基于约定的视图-视图模型解析接口：

```csharp
public interface IViewModelConventionResolver
{
    /// <summary>
    /// 解析视图的视图模型类型
    /// </summary>
    /// <param name="viewType">视图类型</param>
    /// <returns>视图模型类型，如果无法解析则返回 null</returns>
    Type? ResolveViewModelType(Type viewType);
}
```

## 🏗️ 架构

该库遵循清晰的架构，具有明确的关注点分离：

### 层次结构

```
┌─────────────────────────────────────────┐
│          应用程序层                      │
│  (Views, ViewModels, Services)          │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│      NavigationHost.Avalonia            │
│  ┌─────────────────────────────────┐   │
│  │   HostManager (Facade)          │   │
│  │   - 统一的导航 API              │   │
│  └──────────┬──────────────────────┘   │
│             │                            │
│  ┌──────────▼──────────────────────┐   │
│  │   内部服务                      │   │
│  │  - HostRegistry                 │   │
│  │  - ViewModelConventionResolver  │   │
│  │  - InstanceFactory              │   │
│  └─────────────────────────────────┘   │
│                                          │
│  ┌─────────────────────────────────┐   │
│  │   UI 控件                       │   │
│  │  - NavigationHost               │   │
│  └─────────────────────────────────┘   │
└──────────────────────────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│    NavigationHost.Abstractions          │
│  (Interfaces & Contracts)               │
└─────────────────────────────────────────┘
```

### 组件说明

- **NavigationHost** - 显示视图并实现 INavigationHost 的 UI 控件
- **HostManager** - 协调所有导航操作的门面服务
- **Abstractions（抽象）** - 定义契约的接口
  - `IHostManager` - 主服务接口
  - `INavigationHost` - 基本导航接口
  - `INavigationAware` - 导航生命周期接口
  - `IHostRegistry` - 宿主注册管理
  - `IViewModelConventionResolver` - 基于约定的解析
  - `IContentAdapter` - 内容适配器接口
  - `IInstanceFactory` - 实例工厂接口
- **内部服务**：
  - `HostRegistry` - 管理导航宿主注册
  - `ViewModelConventionResolver` - 基于约定的视图-视图模型解析
  - `InstanceFactory` - 通过 DI 容器创建实例
- **Extensions（扩展）** - DI 集成助手
  - `ServiceCollectionExtensions` - 服务注册扩展
- **Adapters（适配器）** - 平台特定的适配器
- **Themes（主题）** - 默认样式和模板

## 📋 完整示例

查看我们的示例项目以获取完整的工作示例：

### 项目结构

```
NavigationHost.Sample.Avalonia/
├── App.axaml                          # 应用程序 XAML
├── App.axaml.cs                       # 应用程序启动和 DI 配置
├── Program.cs                         # 程序入口点
├── ViewLocator.cs                     # 视图定位器（可选）
│
├── ViewModels/                        # 视图模型
│   ├── ViewModelBase.cs               # 基类
│   ├── MainWindowViewModel.cs         # 主窗口视图模型
│   ├── HomeViewModel.cs               # 首页视图模型
│   ├── ProductListViewModel.cs        # 产品列表视图模型
│   ├── ProductDetailViewModel.cs      # 产品详情视图模型
│   ├── SettingsViewModel.cs           # 设置视图模型
│   └── UserProfileViewModel.cs        # 用户资料视图模型
│
└── Views/                             # 视图
    ├── MainWindow.axaml               # 主窗口
    ├── HomeView.axaml                 # 首页视图
    ├── ProductListView.axaml          # 产品列表视图
    ├── ProductDetailView.axaml        # 产品详情视图
    ├── SettingsView.axaml             # 设置视图
    └── UserProfileView.axaml          # 用户资料视图
```

### 运行示例

```bash
cd samples/NavigationHost.Sample.Avalonia
dotnet run
```

示例应用程序展示了：
- ✅ 多个导航宿主的使用
- ✅ 视图之间的导航
- ✅ 带参数的导航
- ✅ INavigationAware 生命周期钩子
- ✅ 依赖注入集成
- ✅ MVVM 模式最佳实践

## 🔧 系统要求

- **.NET Standard 2.0** 或更高版本
- **.NET 6.0**、**.NET 8.0** 或更高版本
- **Avalonia 11.3.10** 或更高版本
- **Microsoft.Extensions.DependencyInjection.Abstractions 8.0.0** 或更高版本（用于 DI 支持）

### 兼容性

| 平台 | 支持 |
|------|------|
| Windows | ✅ |
| Linux | ✅ |
| macOS | ✅ |
| Browser (WASM) | ✅ |
| iOS | ✅ |
| Android | ✅ |

## 🎓 最佳实践

### 1. 使用依赖注入

始终通过 DI 容器注册视图和视图模型，而不是手动创建实例：

```csharp
// ✅ 推荐
services.AddTransient<HomeView>();
services.AddTransient<HomeViewModel>();

// ❌ 不推荐
var view = new HomeView();
var viewModel = new HomeViewModel();
```

### 2. 遵循命名约定

使用一致的命名约定以利用自动解析：

```csharp
// ✅ 推荐
HomeView.axaml → HomeViewModel.cs
ProductListView.axaml → ProductListViewModel.cs

// ❌ 不推荐
Home.axaml → HomeVm.cs
ProductList.axaml → ProductViewModel.cs
```

### 3. 实现 INavigationAware

对于需要感知导航的视图模型，实现 INavigationAware 接口：

```csharp
// ✅ 推荐
public class MyViewModel : ObservableObject, INavigationAware
{
    public void OnNavigatedTo(object? parameter) { /* 初始化 */ }
    public void OnNavigatedFrom() { /* 清理 */ }
    public bool CanNavigateTo(object? parameter) => true;
    public bool CanNavigateFrom() => true;
}
```

### 4. 资源清理

始终在 OnNavigatedFrom 中清理资源：

```csharp
public void OnNavigatedFrom()
{
    // 取消订阅事件
    _subscription?.Dispose();
    
    // 取消异步操作
    _cts?.Cancel();
    _cts?.Dispose();
    
    // 清理集合
    Items.Clear();
}
```

### 5. 参数验证

在 CanNavigateTo 中验证参数：

```csharp
public bool CanNavigateTo(object? parameter)
{
    // 验证参数类型和有效性
    if (parameter is not int id || id <= 0)
    {
        return false;
    }
    return true;
}
```

## 🤝 贡献

我们欢迎各种形式的贡献！

### 如何贡献

1. Fork 本仓库
2. 创建您的功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交您的更改 (`git commit -m '添加一些很棒的功能'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启一个 Pull Request

### 贡献指南

- 遵循现有的代码风格
- 为新功能添加单元测试
- 更新相关文档
- 确保所有测试通过

## 📄 许可证

该项目采用 MIT 许可证 - 详情请参阅 [LICENSE](LICENSE) 文件。

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

## 🙏 致谢

- 灵感来自 [Prism](https://prismlibrary.com/) 的 RegionManager 模式
- 感谢所有贡献者的辛勤工作

## 📚 相关资源

- [Avalonia 官方文档](https://docs.avaloniaui.net/)
- [MVVM 模式指南](https://docs.avaloniaui.net/docs/concepts/the-mvvm-pattern/)
- [依赖注入最佳实践](https://docs.microsoft.com/zh-cn/dotnet/core/extensions/dependency-injection)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/zh-cn/dotnet/communitytoolkit/mvvm/)

## 💬 支持

如果您遇到任何问题或有疑问，请通过以下方式寻求帮助：

- 📝 [提交 Issue](https://github.com/Wenczeslaw/Avalonia.Navigation/issues)
- 💬 [讨论区](https://github.com/Wenczeslaw/Avalonia.Navigation/discussions)
- 📧 通过 GitHub 联系维护者

## 🗺️ 路线图

<div align="center">

**如果这个项目对您有帮助，请给我们一个 ⭐️ Star！**

[报告 Bug](https://github.com/Wenczeslaw/Avalonia.Navigation/issues) · [请求功能](https://github.com/Wenczeslaw/Avalonia.Navigation/issues) · [文档](https://github.com/Wenczeslaw/Avalonia.Navigation)

Made with ❤️ by the NavigationHost.Avalonia team

</div>

