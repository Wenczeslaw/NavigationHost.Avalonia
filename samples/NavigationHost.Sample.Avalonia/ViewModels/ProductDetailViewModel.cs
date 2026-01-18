using CommunityToolkit.Mvvm.ComponentModel;
using NavigationHost.Abstractions;

namespace NavigationHost.Sample.Avalonia.ViewModels;

/// <summary>
/// Demonstrates receiving navigation parameters
/// </summary>
public partial class ProductDetailViewModel : ViewModelBase, INavigationAware
{
    [ObservableProperty]
    private string _title = "Product Detail";

    [ObservableProperty]
    private int _productId;

    [ObservableProperty]
    private string _productName = string.Empty;

    [ObservableProperty]
    private string _productCategory = string.Empty;

    [ObservableProperty]
    private decimal _productPrice;

    [ObservableProperty]
    private string _navigationInfo = string.Empty;

    // INavigationAware implementation
    public bool CanNavigateTo(object? parameter)
    {
        // Validate that we have a valid product parameter
        return parameter is ProductItem;
    }

    public void OnNavigatedTo(object? parameter)
    {
        if (parameter is ProductItem product)
        {
            ProductId = product.Id;
            ProductName = product.Name;
            ProductCategory = product.Category;
            ProductPrice = product.Price;
            Title = $"Product Detail - {product.Name}";
            NavigationInfo = $"Received product parameter: ID={product.Id}, Name={product.Name}";
        }
        else
        {
            NavigationInfo = "No product parameter received";
        }
    }

    public bool CanNavigateFrom()
    {
        return true;
    }

    public void OnNavigatedFrom()
    {
        // Cleanup
    }
}

