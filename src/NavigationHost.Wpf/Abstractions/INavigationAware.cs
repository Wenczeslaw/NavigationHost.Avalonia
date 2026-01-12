// This file is kept for backward compatibility.
// The actual interface is now defined in NavigationHost.Abstractions.
// This is a type forwarding alias.

using AbstractionsNS = NavigationHost.Abstractions;

namespace NavigationHost.WPF.Abstractions
{
    /// <summary>
    ///     Interface for ViewModels that need to receive navigation parameters and participate in navigation lifecycle.
    ///     Implement this interface to handle navigation events and control navigation flow.
    /// </summary>
    public interface INavigationAware : AbstractionsNS.INavigationAware
    {
    }
}

