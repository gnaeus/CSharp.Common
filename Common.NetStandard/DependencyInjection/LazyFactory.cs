using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface ILazy<out T>
    {
        T Value { get; }
    }

    public interface IFactory<out T>
    {
        T Create();
    }

    internal class LazyService<T> : Lazy<T>, ILazy<T>
    {
        public LazyService(IServiceProvider provider)
            : base(() => provider.GetRequiredService<T>()) { }
    }
    
    public class Factory<T> : IFactory<T>
    {
        private readonly IServiceProvider _provider;

        public Factory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public T Create()
        {
            return _provider.GetRequiredService<T>();
        }
    }

    public static class LazyFactoryExtensions
    {
        public static void AddLazy(this IServiceCollection services)
        {
            services.AddTransient(typeof(Lazy<>), typeof(LazyService<>));
            services.AddTransient(typeof(ILazy<>), typeof(LazyService<>));
        }

        public static void AddFactory(this IServiceCollection services)
        {
            services.AddTransient(typeof(Factory<>));
            services.AddTransient(typeof(IFactory<>), typeof(Factory<>));
        }

        public static void AddLazyFactory(this IServiceCollection services)
        {
            services.AddLazy();
            services.AddFactory();
        }
    }
}
