using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using NavigationHost.Sample.WPF.ViewModels;
using NavigationHost.Sample.WPF.Views;
using NavigationHost.WPF.Extensions;

namespace NavigationHost.Sample.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register NavigationHost services
        services.AddNavigationHost();

        // Register ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<ProductListViewModel>();
        services.AddTransient<ProductDetailViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<UserProfileViewModel>();

        // Register Views
        services.AddTransient<MainWindow>();
        services.AddTransient<HomeView>();
        services.AddTransient<ProductListView>();
        services.AddTransient<ProductDetailView>();
        services.AddTransient<SettingsView>();
        services.AddTransient<UserProfileView>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}

