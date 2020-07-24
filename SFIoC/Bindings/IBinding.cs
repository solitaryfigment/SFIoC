using System;
using System.Collections.Generic;

namespace SF.IoC
{
    public interface IBinding : IDisposable
    {
        Type TypeBoundFrom { get; }
        Type TypeBoundTo { get; }
        string ProxyCategory { get; }
        Delegate FactoryMethod { get; }
        bool IsProxy { get; }

        IBinding AsProxy(string category = null);
        IBinding AsSingleton();
        IBinding AsTransient();
        IBinding ToFunction<T>(Func<T> factoryMethod);
        bool HasInstanceAvailable();
        object Resolve(object resolveOnto, Dependency dependency);
        object Resolve(object resolveOnto, Dependency dependency, params object[] args);
        List<Dependency> GetDependencies();
    }
}