using System;
using Microsoft.Extensions.DependencyInjection;

partial class _Examples
{
    class Program
    {
        static void EntryPoint()
        {
            IServiceCollection services = new ServiceCollection();

            ConfigureServices(services);

            IServiceProvider provider = services.BuildServiceProvider();

            using (IServiceScope scope = provider.CreateScope())
            {
                var application = scope.ServiceProvider.GetRequiredService<Application>();

                application.Execute();
            }
        }

        static void ConfigureServices(IServiceCollection services)
        {
            // services.AddLazy();
            // services.AddFactory();
            services.AddLazyFactory();
            
            services.AddTransient<Application>();
            services.AddTransient<Service>();
            services.AddTransient<NativeResource>();

            services.AddScoped<Reporitory>();
        }
    }

    // [Console]
    // Service Created
    // Application Created
    // NativeResource Created
    // Reporitory Created
    // NativeResource Disposed
    // Application.Execute Executed
    // NativeResource Disposed
    // Reporitory Disposed

    class Application
    {
        readonly Service _service;
        readonly ILazy<Reporitory> _repository;
        readonly IFactory<NativeResource> _nativeResourceFactory;

        public Application(
            Service service,
            ILazy<Reporitory> repository,
            IFactory<NativeResource> nativeResourceFactory)
        {
            _service = service;
            _repository = repository;
            _nativeResourceFactory = nativeResourceFactory;

            Console.WriteLine($"{GetType().Name} Created");
        }

        public void Execute()
        {
            using (_nativeResourceFactory.Create())
            {
                var repository = _repository.Value;
            }

            Console.WriteLine($"{GetType().Name}.{nameof(Execute)} Executed");
        }
    }

    class Service
    {
        public Service()
        {
            Console.WriteLine($"{GetType().Name} Created");
        }
    }

    class Reporitory : IDisposable
    {
        public Reporitory()
        {
            Console.WriteLine($"{GetType().Name} Created");
        }

        public void Dispose()
        {
            Console.WriteLine($"{GetType().Name} Disposed");
        }
    }

    class NativeResource : IDisposable
    {
        public NativeResource()
        {
            Console.WriteLine($"{GetType().Name} Created");
        }

        public void Dispose()
        {
            Console.WriteLine($"{GetType().Name} Disposed");
        }
    }
}
