using System;
using System.Linq;
using NavigationHost.Abstractions;

namespace NavigationHost.WPF.Services.Internal
{
    /// <summary>
    ///     Internal service that resolves view model types from view types.
    ///     Supports explicit mappings that take precedence over convention-based resolution.
    ///     Convention: ViewName -> ViewModelName (e.g., HomeView -> HomeViewModel)
    /// </summary>
    internal sealed class ViewModelConventionResolver : IViewModelConventionResolver
    {
        private const string ViewSuffix = "View";
        private const string ViewModelSuffix = "ViewModel";

        private readonly IViewModelMappingRegistry _mappingRegistry;

        /// <summary>
        ///     Initializes a new instance of the ViewModelConventionResolver.
        /// </summary>
        /// <param name="mappingRegistry">The mapping registry for explicit View-ViewModel mappings.</param>
        public ViewModelConventionResolver(IViewModelMappingRegistry mappingRegistry)
        {
            _mappingRegistry = mappingRegistry ?? throw new ArgumentNullException(nameof(mappingRegistry));
        }

        /// <summary>
        ///     Attempts to resolve the view model type for a given view type.
        ///     First checks explicit mappings, then falls back to convention-based resolution.
        /// </summary>
        /// <param name="viewType">The view type.</param>
        /// <returns>The view model type if resolved; otherwise, null.</returns>
        public Type? ResolveViewModelType(Type viewType)
        {
            if (viewType == null)
                return null;

            // 1. Check explicit mapping first (highest priority)
            var explicitMapping = _mappingRegistry.GetViewModelType(viewType);
            if (explicitMapping != null)
                return explicitMapping;

            // 2. Fall back to convention-based resolution
            // Get the view type name
            var viewTypeName = viewType.Name;

            // Check if the view name ends with "View"
            if (!viewTypeName.EndsWith(ViewSuffix, StringComparison.Ordinal))
                return null;

            // Replace "View" with "ViewModel"
            var viewModelName = viewTypeName.Substring(0, viewTypeName.Length - ViewSuffix.Length) + ViewModelSuffix;

            // Try to find the ViewModel type in the same assembly
            var viewModelType = viewType.Assembly.GetTypes()
                .FirstOrDefault(t => t.Name == viewModelName);

            if (viewModelType != null)
                return viewModelType;

            // Try with full namespace replacement (Views -> ViewModels)
            if (viewType.Namespace != null && viewType.Namespace.Contains(".Views"))
            {
                var viewModelNamespace = viewType.Namespace.Replace(".Views", ".ViewModels");
                var fullViewModelName = $"{viewModelNamespace}.{viewModelName}";

                viewModelType = viewType.Assembly.GetTypes()
                    .FirstOrDefault(t => t.FullName == fullViewModelName);
            }

            return viewModelType;
        }

        /// <summary>
        ///     Checks if a view model type can be resolved for the given view type.
        /// </summary>
        /// <param name="viewType">The view type.</param>
        /// <returns>True if a view model type can be resolved; otherwise, false.</returns>
        public bool CanResolve(Type viewType)
        {
            return ResolveViewModelType(viewType) != null;
        }
    }
}

