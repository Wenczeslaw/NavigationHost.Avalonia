using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using NavigationHost.Abstractions;

namespace NavigationHost.Sample.Avalonia.ViewModels;

/// <summary>
/// Example ViewModel demonstrating IAsyncNavigationAware interface usage.
/// This is useful when you need to perform async operations during navigation lifecycle,
/// such as loading data from a database, API calls, or other async initialization.
/// </summary>
public partial class AsyncExampleViewModel : ViewModelBase, IAsyncNavigationAware
{
    [ObservableProperty]
    private string _title = "Async Navigation Example";

    [ObservableProperty]
    private string _statusMessage = "Initializing...";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _loadedData;

    /// <summary>
    /// Called before navigation to this view. Return false to cancel navigation.
    /// </summary>
    public async Task<bool> CanNavigateToAsync(object? parameter)
    {
        // Example: Check if we can navigate (e.g., check permissions, validate parameter)
        await Task.Delay(100); // Simulate async check
        
        // You could check user permissions, validate the parameter, etc.
        return true; // Allow navigation
    }

    /// <summary>
    /// Called when navigating to this view. Perform async initialization here.
    /// </summary>
    public async Task OnNavigatedToAsync(object? parameter)
    {
        IsLoading = true;
        StatusMessage = "Loading data...";

        try
        {
            // Simulate loading data from an API or database
            await Task.Delay(1000); // Simulate network delay
            
            if (parameter != null)
            {
                LoadedData = $"Loaded with parameter: {parameter}";
            }
            else
            {
                LoadedData = "Loaded without parameters";
            }

            StatusMessage = "Data loaded successfully!";
        }
        catch
        {
            StatusMessage = "Failed to load data";
            LoadedData = null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Called before navigating away from this view. Return false to cancel navigation.
    /// </summary>
    public async Task<bool> CanNavigateFromAsync()
    {
        // Example: Check if there are unsaved changes
        if (IsLoading)
        {
            // Don't allow navigation while loading
            return false;
        }

        // You could show a confirmation dialog here
        await Task.Delay(50); // Simulate async check
        
        return true; // Allow navigation away
    }

    /// <summary>
    /// Called when navigating away from this view. Perform cleanup here.
    /// </summary>
    public async Task OnNavigatedFromAsync()
    {
        StatusMessage = "Cleaning up...";
        
        // Perform async cleanup operations
        await Task.Delay(100); // Simulate async cleanup
        
        // Cancel ongoing operations, save state, etc.
        LoadedData = null;
        StatusMessage = "Cleaned up";
    }
}
