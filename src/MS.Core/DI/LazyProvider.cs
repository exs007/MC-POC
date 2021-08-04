using System;
using Microsoft.Extensions.DependencyInjection;

namespace MS.Core.DI
{
    public class LazyProvider<T> : Lazy<T> where T : class
    {
        public LazyProvider(IServiceProvider provider)
            : base(provider.GetRequiredService<T>)
        {
        }
    }
}