# Version 2.1.1 Release Notes

## 发布日期
2026年2月2日

## 新功能和改进

### 1. HostExists() 方法 ✨

所有三个包（Abstractions、WPF、Avalonia）现在都支持 `HostExists()` 方法：

```csharp
bool HostExists(string hostName);
```

**用途：**
- 检查指定名称的导航宿主是否已注册
- 在导航前进行防御性检查
- 实现条件导航逻辑

**示例：**
```csharp
if (hostManager.HostExists("MainHost"))
{
    hostManager.Navigate("MainHost", typeof(HomeView));
}
else
{
    // 处理宿主未就绪的情况
    logger.LogWarning("Host 'MainHost' not ready yet");
}
```

### 2. RequestNavigate 扩展方法（Prism风格）✨

为WPF和Avalonia添加了类似Prism的 `RequestNavigate` 扩展方法：

**命名空间：**
- WPF: `NavigationHost.WPF.Extensions`
- Avalonia: `NavigationHost.Avalonia.Extensions`

**方法签名：**
```csharp
// 同步版本
void RequestNavigate(
    this IHostManager hostManager,
    string hostName,
    Type contentType,
    object? parameter = null,
    Action<NavigationResult>? onComplete = null,
    bool retryOnHostNotReady = true);

// 泛型版本
void RequestNavigate<T>(
    this IHostManager hostManager,
    string hostName,
    object? parameter = null,
    Action<NavigationResult>? onComplete = null,
    bool retryOnHostNotReady = true);

// 异步版本
Task<NavigationResult> RequestNavigateAsync(
    this IHostManager hostManager,
    string hostName,
    Type contentType,
    object? parameter = null,
    bool retryOnHostNotReady = true);

// 泛型异步版本
Task<NavigationResult> RequestNavigateAsync<T>(
    this IHostManager hostManager,
    string hostName,
    object? parameter = null,
    bool retryOnHostNotReady = true);
```

**NavigationResult 类：**
```csharp
public class NavigationResult
{
    public bool Success { get; set; }
    public Exception? Error { get; set; }
}
```

**特性：**
- ✅ 自动处理宿主未就绪情况
- ✅ 支持自动重试（可配置）
- ✅ 提供导航结果回调
- ✅ 类似Prism的API设计
- ✅ 支持同步和异步模式

**示例：**
```csharp
using NavigationHost.WPF.Extensions;

// 使用回调
hostManager.RequestNavigate(
    "MainHost",
    typeof(HomeView),
    onComplete: result =>
    {
        if (!result.Success)
            logger.LogError(result.Error, "Navigation failed");
    },
    retryOnHostNotReady: true
);

// 使用异步
var result = await hostManager.RequestNavigateAsync<HomeView>(
    "MainHost",
    retryOnHostNotReady: true
);

if (!result.Success)
{
    ShowError(result.Error?.Message);
}
```

### 3. WPF: 修复重复导航生命周期问题 🐛

**问题：**
在之前的版本中，当重复导航到同一页面时，如果View已有DataContext，`OnNavigatedTo` 只在第一次调用。

**修复：**
- 现在每次导航都会正确调用 `OnNavigatedTo` 和 `OnNavigatedFrom`
- 即使View的DataContext已存在，也会调用生命周期方法
- 支持页面数据刷新和参数更新

**影响：**
```csharp
// 第一次导航
hostManager.Navigate("MainHost", typeof(HomeView));
// → HomeViewModel.OnNavigatedTo() ✓

// 导航到其他页面
hostManager.Navigate("MainHost", typeof(SettingsView));
// → HomeViewModel.OnNavigatedFrom() ✓
// → SettingsViewModel.OnNavigatedTo() ✓

// 再次导航回HomeView
hostManager.Navigate("MainHost", typeof(HomeView));
// → SettingsViewModel.OnNavigatedFrom() ✓
// → HomeViewModel.OnNavigatedTo() ✓ 现在会正确调用！
```

### 4. WPF: 改进内容对齐 🎨

**问题：**
NavigationHost 中显示的视图可能大小不正确，不会填充整个可用空间。

**修复：**
- 在 `Generic.xaml` 中为 `HorizontalContentAlignment` 和 `VerticalContentAlignment` 添加了默认值 `Stretch`
- 导航到的视图现在会自动拉伸以填充 NavigationHost 的全部空间
- 与 Avalonia 版本的行为保持一致

**变更：**
```xml
<Style TargetType="{x:Type local:NavigationHost}">
    <Setter Property="HorizontalContentAlignment" Value="Stretch" />
    <Setter Property="VerticalContentAlignment" Value="Stretch" />
    <!-- ... -->
</Style>
```

## 文档更新

### 更新文档

- **README.MD** - 添加了新API的完整说明和示例
- **README.zh-CN.MD** - 添加了新API的中文说明和示例

## 升级指南

### 从 2.1.0 升级到 2.1.1

1. **更新包版本：**
```xml
<PackageReference Include="NavigationHost.Abstractions" Version="2.1.1" />
<PackageReference Include="NavigationHost.WPF" Version="2.1.1" />
<!-- 或 -->
<PackageReference Include="NavigationHost.Avalonia" Version="2.1.1" />
```

2. **可选：使用 RequestNavigate**

如果遇到导航时序问题，可以使用新的 RequestNavigate 方法：

```csharp
// 之前（可能遇到 "No host registered" 错误）
hostManager.Navigate("MainHost", typeof(HomeView));

// 现在（自动处理宿主未就绪）
hostManager.RequestNavigate("MainHost", typeof(HomeView), 
    retryOnHostNotReady: true);
```

3. **重复导航的注意事项（仅WPF）**

如果您的代码依赖于 `OnNavigatedTo` 只被调用一次，需要调整：

```csharp
// 之前（可能有问题）
public class HomeViewModel : INavigationAware
{
    private bool _initialized = false;
    
    public void OnNavigatedTo(object? parameter)
    {
        if (!_initialized)
        {
            LoadInitialData();
            _initialized = true;
        }
    }
}

// 修复后
public class HomeViewModel : INavigationAware
{
    public HomeViewModel()
    {
        // 一次性初始化放在构造函数中
        LoadInitialData();
    }
    
    public void OnNavigatedTo(object? parameter)
    {
        // 每次导航都刷新数据
        LoadData(parameter);
    }
}
```

## 破坏性变更

**无破坏性变更** - 此版本完全向后兼容 2.1.x。

所有新功能都是新增的，现有API的行为保持不变（除了修复的bug）。

## 下一步计划

- [ ] 添加导航历史管理（前进/后退）
- [ ] 添加导航动画支持
- [ ] 改进性能和内存使用
- [ ] 更多单元测试覆盖

## 支持

如有问题或建议，请：
- 📝 [提交 Issue](https://github.com/Wenczeslaw/NavigationHost/issues)
- 💬 [参与讨论](https://github.com/Wenczeslaw/NavigationHost/discussions)

---

**完整更新日志：**

### NavigationHost.Abstractions 2.1.1
- 新增 `HostExists()` 方法到 `IHostManager` 接口

### NavigationHost.WPF 2.1.1
- 新增 `HostExists()` 方法实现
- 新增 `RequestNavigate` 扩展方法（Prism风格）
- 修复重复导航时 `OnNavigatedTo` 不被调用的问题
- 改进内容对齐，默认使用 Stretch 对齐

### NavigationHost.Avalonia 2.1.1
- 新增 `HostExists()` 方法实现
- 新增 `RequestNavigate` 扩展方法（Prism风格）
