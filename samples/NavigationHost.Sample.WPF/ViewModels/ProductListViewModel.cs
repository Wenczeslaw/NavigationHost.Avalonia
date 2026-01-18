using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NavigationHost.Abstractions;
using NavigationHost.Sample.WPF.Views;

namespace NavigationHost.Sample.WPF.ViewModels;

/// <summary>
/// Demonstrates navigation with parameters and INavigationAware interface
/// </summary>
public partial class ProductListViewModel : ViewModelBase, INavigationAware
{
    private readonly IHostManager _hostManager;

    [ObservableProperty]
    private string _title = "Product List";

    [ObservableProperty]
    private string _categoryFilter = "All Categories";

    [ObservableProperty]
    private ObservableCollection<ProductItem> _products = [];

    [ObservableProperty]
    private ProductItem? _selectedProduct;

    public ProductListViewModel(IHostManager hostManager)
    {
        _hostManager = hostManager;
        LoadProducts();
    }

    private void LoadProducts()
    {
        Products.Clear();
        Products.Add(new ProductItem { Id = 1, Name = "Laptop", Category = "Electronics", Price = 999.99m });
        Products.Add(new ProductItem { Id = 2, Name = "Mouse", Category = "Electronics", Price = 29.99m });
        Products.Add(new ProductItem { Id = 3, Name = "Keyboard", Category = "Electronics", Price = 79.99m });
        Products.Add(new ProductItem { Id = 4, Name = "Monitor", Category = "Electronics", Price = 299.99m });
        Products.Add(new ProductItem { Id = 5, Name = "Headphones", Category = "Audio", Price = 149.99m });
    }

    [RelayCommand]
    private void ViewProductDetail()
    {
        if (SelectedProduct != null)
        {
            _hostManager.Navigate<ProductDetailView>("MainHost", parameter: SelectedProduct);
        }
    }

    // INavigationAware implementation
    public bool CanNavigateTo(object? parameter)
    {
        // Always allow navigation to this view
        return true;
    }

    public void OnNavigatedTo(object? parameter)
    {
        // Handle navigation parameter
        if (parameter is string category)
        {
            CategoryFilter = category;
            Title = $"Product List - {category}";
        }
        else
        {
            CategoryFilter = "All Categories";
            Title = "Product List";
        }
    }

    public bool CanNavigateFrom()
    {
        // Always allow navigation away
        return true;
    }

    public void OnNavigatedFrom()
    {
        // Cleanup when leaving
        SelectedProduct = null;
    }
}

public class ProductItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
