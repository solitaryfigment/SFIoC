using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SF.IoC
{
    public class Binding<T1,T2> : IBinding where T1 : class where T2 : class
    {
        private T1 _instance;
        private BingingType _bingingType = BingingType.Transient;
        private List<Dependency> _dependencies = null;
        
        public Type TypeBoundFrom
        {
            get { return typeof(T1); }
        }
        
        public Type TypeBoundTo
        {
            get { return typeof(T2); }
        }

        public Binding()
        {
            if(!TypeBoundFrom.IsAssignableFrom(TypeBoundTo))
            {
                throw new BindingException($"Type {TypeBoundTo.Name} must inherit from {TypeBoundFrom.Name}.");
            }
        }

        public Binding(T2 instance) : this()
        {
            if(instance != null)
            {
                _instance = instance as T1;
            }
        }

        public bool HasInstanceAvailable()
        {
            return _instance != null;
        }
        
        public IBinding AsSingleton()
        {
            _bingingType = BingingType.Singleton;
            return this;
        }

        public IBinding AsTransient()
        {
            _bingingType = BingingType.Transient;
            return this;
        }

        public List<Dependency> GetDependencies()
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
                    
                    constructorDependencies.Add(new Dependency
                    {
                        MemberName = parameterInfo.Name,
                        MemberType = MemberTypes.Constructor,
                        Type = parameterInfo.ParameterType,
                        Category = (attribute == null) ? "" : attribute.Category
                    });
                }

                if(constructorDependencies.Count > 0)
                {
                    _dependencies.Add(new ConstructorDependency
                    {
                        MemberName = "Constructor",
                        MemberType = MemberTypes.Constructor,
                        ArgumentDependencies = constructorDependencies,
                    });
                }
            }
            else if (constructorInfos.Count > 1)
            {
                throw new Exception($"Type {TypeBoundTo.Name} cannot contain 2 DefaultConstructors.");
            }
            
            foreach(var member in injectedMembers)
            {
                var property = member as PropertyInfo;
                if(property != null)
                {
                    _dependencies.Add(new Dependency
                    {
                        MemberName = property.Name,
                        MemberType = MemberTypes.Property,
                        Category = property.GetCustomAttribute<InjectAttribute>(true).Category,
                        Type = property.PropertyType
                    });
                }
                else
                {
                    var field = member as FieldInfo;
                    if(field != null)
                    {
                        _dependencies.Add(new Dependency
                        {
                            MemberName = field.Name,
                            MemberType = MemberTypes.Field,
                            Category = field.GetCustomAttribute<InjectAttribute>(true).Category,
                            Type = field.FieldType
                        });
                    }
                }
            }

            return _dependencies;
        }

        public object Resolve()
        {
            return Resolve(null);
        }

        public object Resolve(params object[] args)
        {
            if(_instance != null)
            {
                return _instance;
            }
            if(_bingingType != BingingType.Singleton)
            {
                if(args != null && args.Length > 0)
                {
                    return Activator.CreateInstance(TypeBoundTo, args) as T1;
                }
                return Activator.CreateInstance<T2>() as T1;
            }

            if(_instance == null && args != null && args.Length > 0)
            {
                _instance = Activator.CreateInstance(TypeBoundTo, args) as T1;
            }
            else if(_instance == null)
            {
                _instance = Activator.CreateInstance<T2>() as T1;
            }

            return _instance;
        }

        public void Dispose()
        {
            var disposable = _instance as IDisposable;
            disposable?.Dispose();
            _dependencies?.Clear();
            _instance = null;
        }
    }
}