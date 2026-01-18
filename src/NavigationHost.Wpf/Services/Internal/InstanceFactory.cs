﻿using System;
using Microsoft.Extensions.DependencyInjection;
using AbstractionsNS = NavigationHost.Abstractions;

namespace NavigationHost.WPF.Services.Internal
{
    /// <summary>
    ///     Internal service responsible for creating instances of views and view models.
    ///     Uses the DI container when available, falls back to Activator.CreateInstance.
    ///     Implements platform-agnostic IInstanceFactory interface.
    /// </summary>
    internal sealed class InstanceFactory : AbstractionsNS.IInstanceFactory
    {
        private readonly IServiceProvider? _serviceProvider;

        public InstanceFactory(IServiceProvider? serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        ///     Creates a view instance from the DI container or Activator.
        /// </summary>
        /// <param name="viewType">The type of view to create.</param>
        /// <returns>A view instance.</returns>
        public object CreateView(Type viewType)
        {
            return CreateInstance(viewType);
        }

        /// <summary>
        ///     Creates a view model instance from the DI container or Activator.
        /// </summary>
        /// <param name="viewModelType">The type of view model to create.</param>
        /// <returns>A view model instance.</returns>
        public object CreateViewModel(Type viewModelType)
        {
            return CreateInstance(viewModelType);
        }

        /// <summary>
        ///     Creates an instance of the specified type.
        /// </summary>
        /// <param name="type">The type to create.</param>
        /// <returns>The created instance.</returns>
        public object CreateInstance(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            // Try to get from DI container first
            if (_serviceProvider != null)
            {
                var instance = _serviceProvider.GetService(type);
                if (instance != null)
                    return instance;
            }

            // Fallback to Activator
            try
            {
                return Activator.CreateInstance(type)
                    ?? throw new InvalidOperationException($"Failed to create instance of type {type.FullName}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to create instance of type {type.FullName}. " +
                    $"Ensure the type has a parameterless constructor or is registered in the DI container.", ex);
            }
        }

        /// <summary>
        ///     Creates an instance of the specified type.
        /// </summary>
        /// <typeparam name="T">The type to create.</typeparam>
        /// <returns>The created instance.</returns>
        public T CreateInstance<T>()
        {
            return (T)CreateInstance(typeof(T));
        }
    }
}

