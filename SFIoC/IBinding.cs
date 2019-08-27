using System;
using System.Collections.Generic;

namespace SF.IoC
{
    public interface IBinding : IDisposable
    {
        Type TypeBoundFrom { get; }
        Type TypeBoundTo { get; }
        IBinding AsSingleton();
        IBinding AsTransient();
        bool HasInstanceAvailable();
        object Resolve();
        object Resolve(params object[] args);
        List<Dependency> GetDependencies();
    }
}