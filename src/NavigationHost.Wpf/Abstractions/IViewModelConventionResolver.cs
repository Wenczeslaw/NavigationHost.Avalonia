// This file is kept for backward compatibility.
// The actual interface is now defined in NavigationHost.Abstractions.
// This is a type forwarding alias.

using AbstractionsNS = NavigationHost.Abstractions;

namespace NavigationHost.WPF.Abstractions
{
    /// <summary>
    ///     Defines the convention-based view model resolver interface.
    ///     Automatically resolves view model types from view types based on naming conventions.
    /// </summary>
    public interface IViewModelConventionResolver : AbstractionsNS.IViewModelConventionResolver
    {
    }
}

