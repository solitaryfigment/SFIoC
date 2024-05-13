using System;
using System.Collections.Generic;
using SFIoC.Utils;

namespace SF.IoC
{
    public abstract class Binding : IBinding
    {
        protected object _instance;
        protected BingingType _bingingType = BingingType.Transient;
        protected List<Dependency> _dependencies = null;

        public BingingType BingingType => _bingingType;

        public Type TypeBoundFrom { get; internal set; }
        public Type TypeBoundTo { get; internal set; }

        protected Binding(Type typeBoundFrom, Type typeBoundTo)
        {
            TypeBoundFrom = typeBoundFrom;
            TypeBoundTo = typeBoundTo;

            if(!TypeBoundFrom.IsAssignableFrom(TypeBoundTo))
            {
                throw new BindingException($"Type {TypeBoundTo.Name} must inherit from {TypeBoundFrom.Name}.");
            }
        }

        protected Binding(Type typeBoundFrom, Type typeBoundTo, object instance) : this(typeBoundFrom, typeBoundTo)
        {
            if(instance != null)
            {
                _instance = instance;
            }
        }

        public virtual bool HasInstanceAvailable()
        {
            return _instance != null;
        }

        public virtual IBinding AsSingleton()
        {
            _bingingType = BingingType.Singleton;
            return this;
        }

        public virtual IBinding AsTransient()
        {
            _bingingType = BingingType.Transient;
            return this;
        }

        public virtual List<Dependency> GetDependencies()
        {
            if(_dependencies != null)
            {
                return _dependencies;
            }

            _dependencies = DependencyFinder.GetDependencies(TypeBoundTo);

            return _dependencies;
        }

        public object Resolve(object resolvingOnto, Dependency resolvingDependency)
        {
            return Resolve(resolvingOnto, resolvingDependency, null);
        }

        public virtual object Resolve(object resolvingOnto, Dependency resolvingDependency, params object[] args)
        {
            if(_instance != null)
            {
                return _instance;
            }
            if(_bingingType != BingingType.Singleton)
            {
                if(args != null && args.Length > 0)
                {
                    return Activator.CreateInstance(TypeBoundTo, args);
                }
                return Activator.CreateInstance(TypeBoundTo);
            }

            if(_instance == null && args != null && args.Length > 0)
            {
                _instance = Activator.CreateInstance(TypeBoundTo, args);
            }
            else if(_instance == null)
            {
                _instance = Activator.CreateInstance(TypeBoundTo);
            }

            return _instance;
        }

        public void Dispose()
        {
            var disposable = _instance as IDisposable;
            disposable?.Dispose();
            _dependencies?.Clear();
            _instance = null;
            OnDispose();
        }

        protected virtual void OnDispose()
        {

        }
    }

    public class Binding<T1,T2> : Binding where T2 : class, T1
    {
        public Binding() : base(typeof(T1), typeof(T2))
        {
        }

        public Binding(T2 instance) : base(typeof(T1), typeof(T2), instance)
        {
        }
    }
}
