using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NavigationHost.Avalonia.Abstractions;

namespace NavigationHost.Avalonia.Services.Internal
{
    /// <summary>
    ///     Internal implementation of convention-based view model resolver.
    ///     Automatically resolves view model types from view types based on naming conventions.
    /// </summary>
    internal sealed class ViewModelConventionResolver : IViewModelConventionResolver
    {
        private const string ViewSuffix = "View";
        private const string ViewModelSuffix = "ViewModel";

        /// <summary>
        ///     Attempts to resolve the view model type for a given view type based on conventions.
        /// </summary>
        /// <param name="viewType">The view type.</param>
        /// <returns>The view model type if resolved by convention; otherwise, null.</returns>
        public Type? ResolveViewModelType(Type viewType)
        {
            if (viewType == null)
                throw new ArgumentNullException(nameof(viewType));


            // Use default convention: ViewName -> ViewModelName
            var viewName = viewType.Name;
            var viewModelName = GetViewModelNameByConvention(viewName);

            if (string.IsNullOrEmpty(viewModelName))
                return null;

            // Search for the view model type

            // 1. Search in the same namespace and assembly as the view
            var viewModelType = SearchInAssembly(viewType.Assembly, viewType.Namespace, viewModelName);
            if (viewModelType != null)
                return viewModelType;

            // 2. Search in all loaded assemblies (if enabled)
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                viewModelType = SearchInAssembly(assembly, null, viewModelName);
                if (viewModelType != null)
                    return viewModelType;
            }


            return null;
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

        /// <summary>
        ///     Gets the view model name by convention from the view name.
        /// </summary>
        /// <param name="viewName">The view name.</param>
        /// <returns>The view model name, or null if convention doesn't apply.</returns>
        private string? GetViewModelNameByConvention(string viewName)
        {
            if (string.IsNullOrEmpty(viewName))
                return null;

            // Check if view name ends with the configured view suffix (e.g., "View")
            if (viewName.EndsWith(ViewSuffix, StringComparison.Ordinal))
            {
                // Case 1: View name ends with view suffix
                // Remove view suffix and add view model suffix
                // Example: "HomeView" -> "Home" + "ViewModel" = "HomeViewModel"
                var baseName = viewName.Substring(0, viewName.Length - ViewSuffix.Length);
                return baseName + ViewModelSuffix;
            }

            // Case 2: View name doesn't end with view suffix
            // Directly append view model suffix to the view name
            // Example: "Home" -> "Home" + "ViewModel" = "HomeViewModel"
            //          "MainWindow" -> "MainWindow" + "ViewModel" = "MainWindowViewModel"
            return viewName + ViewModelSuffix;
        }

        /// <summary>
        ///     Searches for a type in the specified assembly and namespace.
        /// </summary>
        /// <param name="assembly">The assembly to search in.</param>
        /// <param name="viewNamespace">The view namespace (used to derive potential ViewModel namespaces).</param>
        /// <param name="typeName">The type name to search for.</param>
        /// <returns>The found type, or null.</returns>
        private Type? SearchInAssembly(Assembly assembly, string? viewNamespace, string typeName)
        {
            try
            {
                // Build a list of potential namespaces to search
                var potentialNamespaces = GetPotentialNamespaces(viewNamespace);

                // Try each potential namespace
                foreach (var namespaceName in potentialNamespaces)
                {
                    if (!string.IsNullOrEmpty(namespaceName))
                    {
                        var fullTypeName = $"{namespaceName}.{typeName}";
                        var type = assembly.GetType(fullTypeName, false);
                        if (type != null)
                            return type;
                    }
                }

                // If all namespace attempts fail, search all types in the assembly by name only
                var types = assembly.GetTypes();
                return types.FirstOrDefault(t => t.Name == typeName);
            }
            catch (ReflectionTypeLoadException)
            {
                // Ignore assemblies that can't be fully loaded
                return null;
            }
            catch (Exception)
            {
                // Ignore other exceptions during type loading
                return null;
            }
        }

        /// <summary>
        ///     Gets potential namespaces for ViewModel based on View namespace conventions.
        /// </summary>
        /// <param name="viewNamespace">The view namespace.</param>
        /// <returns>A list of potential namespaces to search for ViewModels.</returns>
        private List<string> GetPotentialNamespaces(string? viewNamespace)
        {
            var namespaces = new List<string>();

            if (string.IsNullOrEmpty(viewNamespace))
                return namespaces;

            // 1. Same namespace as View (e.g., MyApp.Views -> MyApp.Views)
            namespaces.Add(viewNamespace);

            // 2. Replace "Views" with "ViewModels" (e.g., MyApp.Views -> MyApp.ViewModels)
            if (viewNamespace.Contains(".Views"))
            {
                namespaces.Add(viewNamespace.Replace(".Views", ".ViewModels"));
            }

            // 3. Replace trailing "Views" with "ViewModels" (e.g., MyApp.Views -> MyApp.ViewModels)
            if (viewNamespace.EndsWith(".Views", StringComparison.Ordinal))
            {
                var baseNamespace = viewNamespace.Substring(0, viewNamespace.Length - 6); // Remove ".Views"
                namespaces.Add(baseNamespace + ".ViewModels");
            }

            // 4. If namespace ends with "Views" (no dot), replace it (e.g., Views -> ViewModels)
            if (viewNamespace.EndsWith("Views", StringComparison.Ordinal) &&
                !viewNamespace.EndsWith(".Views", StringComparison.Ordinal))
            {
                var baseNamespace = viewNamespace.Substring(0, viewNamespace.Length - 5); // Remove "Views"
                namespaces.Add(baseNamespace + "ViewModels");
            }

            // 5. Parent namespace + ViewModels (e.g., MyApp.Views.Home -> MyApp.ViewModels)
            var lastDotIndex = viewNamespace.LastIndexOf('.');
            if (lastDotIndex > 0)
            {
                var parentNamespace = viewNamespace.Substring(0, lastDotIndex);

                // If parent ends with something, try replacing it with ViewModels
                if (parentNamespace.EndsWith(".Views", StringComparison.Ordinal))
                {
                    var baseParent = parentNamespace.Substring(0, parentNamespace.Length - 6);
                    namespaces.Add(baseParent + ".ViewModels");
                }
                else
                {
                    // Just append .ViewModels to parent
                    namespaces.Add(parentNamespace + ".ViewModels");
                }
            }

            // 6. Replace any occurrence of "Views" with "ViewModels" in the namespace path
            var parts = viewNamespace.Split('.');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "Views")
                {
                    var modifiedParts = parts.ToArray();
                    modifiedParts[i] = "ViewModels";
                    namespaces.Add(string.Join(".", modifiedParts));
                }
            }

            // Remove duplicates and return
            return namespaces.Distinct().ToList();
        }
    }
}