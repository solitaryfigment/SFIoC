using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SF.IoC
{
    public abstract class Binding : IBinding
    {
        protected object _instance;
        protected BingingType _bingingType = BingingType.Transient;
        protected List<Dependency> _dependencies = null;
        
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
            
            _dependencies = new List<Dependency>();
            var members = TypeBoundTo.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var injectedMembers = members.Where(m => m.GetCustomAttributes(true).Any(a => a is InjectAttribute));

            var constructorInfos = new List<ConstructorInfo>();
            constructorInfos.AddRange(TypeBoundTo.GetConstructors().Where(c => c.GetCustomAttributes(true).Any(a => a is DefaultConstructorAttribute)));
            if(constructorInfos.Count == 0)
            {
                var type = TypeBoundTo.BaseType;
                while(type!= null && type != typeof(object) && constructorInfos.Count == 0)
                {
                    constructorInfos.AddRange(type.GetConstructors().Where(c => c.GetCustomAttributes(true).Any(a => a is DefaultConstructorAttribute)));
                    type = type.BaseType;
                }
            }
            
            var constructorDependencies = new List<Dependency>();

            if(constructorInfos.Count == 1)
            {
                var constructorInfo = constructorInfos[0];
                var parameterInfos = constructorInfo.GetParameters();
                    
                foreach(var parameterInfo in parameterInfos)
                {
                    var attribute = parameterInfo.GetCustomAttribute<InjectArgumentAttribute>(true);

                    if(attribute != null)
                    {
                        constructorDependencies.Add(attribute.CreateDependency(parameterInfo.Name, MemberTypes.Constructor, parameterInfo.ParameterType));
                    }
                    else
                    {
                        constructorDependencies.Add(new Dependency
                        {
                            MemberName = parameterInfo.Name,
                            MemberType = MemberTypes.Constructor,
                            Type = parameterInfo.ParameterType
                        });
                    }
                }

                if(constructorDependencies.Count > 0)
                {
                    var attribute = constructorInfo.GetCustomAttribute<DefaultConstructorAttribute>(true);
                    _dependencies.Add(attribute.CreateDependency("Constructor", MemberTypes.Constructor, constructorDependencies));
                }
            }
            else if (constructorInfos.Count > 1)
            {
                throw new Exception($"Type {TypeBoundTo.Name} cannot contain 2 DefaultConstructors.");
            }
            
            foreach(var member in injectedMembers)
            {
                var property = member as PropertyInfo;
                var attribute = member.GetCustomAttribute<InjectAttribute>(true);
                if(!attribute.CanBeUsedOnType(TypeBoundTo))
                {
                    // TODO: Log error and continue
                    continue;
                }
                if(property != null)
                {
                    _dependencies.Add(attribute.CreateDependency(property.Name, MemberTypes.Property, property.PropertyType));
                }
                else
                {
                    var field = member as FieldInfo;
                    if(field != null)
                    {
                        _dependencies.Add(attribute.CreateDependency(field.Name, MemberTypes.Field, field.FieldType));
                    }
                }
            }

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

    public class Binding<T1,T2> : Binding where T1 : class where T2 : class
    {
        public Binding() : base(typeof(T1), typeof(T2))
        {
        }

        public Binding(T2 instance) : base(typeof(T1), typeof(T2), instance)
        {
        }
    }
}