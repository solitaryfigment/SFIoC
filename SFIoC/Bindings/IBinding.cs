using System;
using System.Collections.Generic;

namespace SF.IoC
{
    public interface IBinding : IDisposable
    {
        BingingType BingingType { get; }
        Type TypeBoundFrom { get; }
        Type TypeBoundTo { get; }
        IBinding AsSingleton();
        IBinding AsTransient();
        bool HasInstanceAvailable();
        object Resolve(object resolveOnto, Dependency dependency);
        object Resolve(object resolveOnto, Dependency dependency, params object[] args);
        List<Dependency> GetDependencies();
    }
}
