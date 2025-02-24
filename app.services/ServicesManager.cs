using app.interfaces;
using System.Collections.Concurrent;

namespace app.services
{
    public sealed class ServicesManager<TService> : IServicesManager<TService> where TService : class
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<Type, Func<TService>> _serviceFactories;
        private readonly ConcurrentDictionary<Type, Lazy<TService>> _serviceCache;

        public ServicesManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _serviceFactories = new ConcurrentDictionary<Type, Func<TService>>(InitializeServiceFactories());
            _serviceCache = new ConcurrentDictionary<Type, Lazy<TService>>();
        }

        private ConcurrentDictionary<Type, Func<TService>> InitializeServiceFactories()
        {
            var serviceFactories = new ConcurrentDictionary<Type, Func<TService>>();
            var assembly = typeof(IServicesManager<TService>).Assembly;

            var interfaceTypes = assembly.GetExportedTypes()
                .Where(t => t.IsInterface &&
                            t.Name.StartsWith("I") &&
                            t.Name.EndsWith("Service") &&
                            t.Namespace.StartsWith("app.interfaces"));

            foreach (var interfaceType in interfaceTypes)
            {
                var implementationType = GetImplementationType(interfaceType);

                if (implementationType != null)
                {
                    serviceFactories[interfaceType] = CreateServiceFactory(implementationType);
                }
                else
                {
                    Console.WriteLine($"No implementation found for {interfaceType.Name}.");
                }
            }

            return serviceFactories;
        }

        private Type GetImplementationType(Type interfaceType)
        {
            var assembly = typeof(ServicesManager<TService>).Assembly;
            return assembly.GetTypes()
                .FirstOrDefault(t => t.IsClass && !t.IsAbstract &&
                                     t.GetInterfaces().Contains(interfaceType));
        }

        private Func<TService> CreateServiceFactory(Type implementationType)
        {
            var constructor = implementationType.GetConstructors().FirstOrDefault()
                ?? throw new InvalidOperationException($"No public constructor found for {implementationType.Name}.");

            var parameters = constructor.GetParameters();
            var resolvedParameters = parameters.Select(param => _serviceProvider.GetService(param.ParameterType)).ToArray();

            return () => (TService)Activator.CreateInstance(implementationType, resolvedParameters);
        }

        public TService GetService(Type serviceType)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            return _serviceCache.GetOrAdd(serviceType, CreateLazyService).Value;
        }

        private Lazy<TService> CreateLazyService(Type serviceType)
        {
            if (_serviceFactories.TryGetValue(serviceType, out var serviceFactory))
            {
                return new Lazy<TService>(() => serviceFactory.Invoke());
            }

            throw new InvalidOperationException($"Service of type {serviceType.Name} is not registered.");
        }
        public TService Service => GetService(typeof(TService));
    }
}
