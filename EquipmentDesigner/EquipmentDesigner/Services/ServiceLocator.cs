using System;
using System.Collections.Generic;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Services
{
    /// <summary>
    /// Simple service locator for dependency injection in WPF application.
    /// Provides singleton and factory-based service registration.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _singletons = new Dictionary<Type, object>();
        private static readonly Dictionary<Type, Func<object>> _factories = new Dictionary<Type, Func<object>>();
        private static readonly object _lock = new object();

        /// <summary>
        /// Registers a singleton instance for the specified service type.
        /// </summary>
        /// <typeparam name="TService">The service interface type.</typeparam>
        /// <param name="instance">The singleton instance to register.</param>
        public static void RegisterSingleton<TService>(TService instance) where TService : class
        {
            lock (_lock)
            {
                _singletons[typeof(TService)] = instance;
            }
        }

        /// <summary>
        /// Registers a factory function for the specified service type.
        /// A new instance is created on each GetService call.
        /// </summary>
        /// <typeparam name="TService">The service interface type.</typeparam>
        /// <param name="factory">The factory function to create instances.</param>
        public static void RegisterFactory<TService>(Func<TService> factory) where TService : class
        {
            lock (_lock)
            {
                _factories[typeof(TService)] = () => factory();
            }
        }

        /// <summary>
        /// Gets the registered service instance.
        /// </summary>
        /// <typeparam name="TService">The service interface type.</typeparam>
        /// <returns>The service instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the service is not registered.</exception>
        public static TService GetService<TService>() where TService : class
        {
            lock (_lock)
            {
                var type = typeof(TService);

                if (_singletons.TryGetValue(type, out var singleton))
                {
                    return (TService)singleton;
                }

                if (_factories.TryGetValue(type, out var factory))
                {
                    return (TService)factory();
                }

                throw new InvalidOperationException($"Service {type.Name} is not registered.");
            }
        }

        /// <summary>
        /// Tries to get the registered service instance.
        /// </summary>
        /// <typeparam name="TService">The service interface type.</typeparam>
        /// <param name="service">The resolved service instance, or null if not found.</param>
        /// <returns>True if the service was found; otherwise, false.</returns>
        public static bool TryGetService<TService>(out TService service) where TService : class
        {
            lock (_lock)
            {
                var type = typeof(TService);

                if (_singletons.TryGetValue(type, out var singleton))
                {
                    service = (TService)singleton;
                    return true;
                }

                if (_factories.TryGetValue(type, out var factory))
                {
                    service = (TService)factory();
                    return true;
                }

                service = null;
                return false;
            }
        }

        /// <summary>
        /// Checks if a service is registered.
        /// </summary>
        /// <typeparam name="TService">The service interface type.</typeparam>
        /// <returns>True if the service is registered; otherwise, false.</returns>
        public static bool IsRegistered<TService>() where TService : class
        {
            lock (_lock)
            {
                var type = typeof(TService);
                return _singletons.ContainsKey(type) || _factories.ContainsKey(type);
            }
        }

        /// <summary>
        /// Clears all registrations. Used for testing.
        /// </summary>
        public static void Reset()
        {
            lock (_lock)
            {
                _singletons.Clear();
                _factories.Clear();
            }
        }

        /// <summary>
        /// Configures services for production use with typed repositories.
        /// </summary>
        public static void ConfigureForProduction()
        {
            // Register typed repositories for multi-file support
            var workflowRepository = new LocalHardwareRepository();
            RegisterSingleton<IWorkflowRepository>(workflowRepository);

            // Unified structure for uploaded workflows
            var uploadedWorkflowRepository = new UploadedWorkflowRepository();
            RegisterSingleton<IUploadedWorkflowRepository>(uploadedWorkflowRepository);

            // Hardware API Service (wraps UploadedWorkflowRepository for REST API semantics)
            var hardwareApiService = new MockHardwareApiService(uploadedWorkflowRepository);
            RegisterSingleton<IHardwareApiService>(hardwareApiService);
        }

        /// <summary>
        /// Configures services for testing with MemoryTypedRepository.
        /// </summary>
        public static void ConfigureForTesting()
        {
            // Register typed memory repositories for multi-file support testing
            var workflowRepository = new MemoryWorkflowRepository();
            RegisterSingleton<IWorkflowRepository>(workflowRepository);

            // Unified structure for uploaded workflows
            var uploadedWorkflowRepository = new MemoryUploadedWorkflowRepository();
            RegisterSingleton<IUploadedWorkflowRepository>(uploadedWorkflowRepository);

            // Hardware API Service for testing
            var hardwareApiService = new MockHardwareApiService(uploadedWorkflowRepository);
            RegisterSingleton<IHardwareApiService>(hardwareApiService);
        }
    }
}