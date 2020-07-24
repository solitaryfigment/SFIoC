using System;
using System.Collections.Generic;
using System.Reflection;

namespace SF.IoC
{
    public class Resolver
    {
        private Type _boundType;
        private string _category;
        private Func<Type, string, IBinding> _getBinding;
        private Action<Type, Type, string, Dependency, object> _preResolve;
        private Dictionary<Type, List<IBinding>> _resolvedBindings = new Dictionary<Type, List<IBinding>>();
        private HashSet<Type> _proxiedBindings = new HashSet<Type>();
        
        public Resolver(Type boundType, string category, Func<Type, string, IBinding> getBinding, Action<Type, Type, string, Dependency, object> preResolve)
        {
            _boundType = boundType;
            _category = category;
            _getBinding = getBinding;
            _preResolve = preResolve;
        }

        public virtual object Resolve()
        {
            return Resolve(_boundType, null, _category);
        }

        protected virtual object Resolve(Type type, Type owner, string category, Dependency resolvingDependency = null, object resolvingOnto = null)
        {
            _preResolve(type, owner, category, resolvingDependency, resolvingOnto);
            var binding = _getBinding(type, category);
            if(binding.HasInstanceAvailable())
            {
                return binding.Resolve(resolvingOnto, resolvingDependency);
            }

            if(binding.IsProxy)
            {
                if(_proxiedBindings.Contains(binding.TypeBoundFrom))
                {
                    throw new CircularProxyException($"Circular dependency detected in {_boundType.Name} on {binding.TypeBoundTo.Name}.");
                }

                _proxiedBindings.Add(binding.TypeBoundFrom);
                return Resolve(binding.TypeBoundTo, null, category);
            }
            else
            {
                _proxiedBindings.Clear();
            }
            
            List<IBinding> bindings = null;
            if(owner != null && !_resolvedBindings.TryGetValue(owner, out bindings))
            {
                bindings = new List<IBinding>();
                _resolvedBindings.Add(owner, bindings);
            }
            
            if(owner != null && bindings != null && bindings.Contains(binding))
            {
                throw new CircularDependencyException($"Circular dependency detected in {owner.Name} on {binding.TypeBoundTo.Name}.");
            }

            if(owner != null)
            {
                bindings?.Add(binding);
            }
            
            var dependencies = binding.GetDependencies();
            ConstructorDependency constructorDependency = null;

            if(dependencies.Count > 0)
            {
                constructorDependency = dependencies[0] as ConstructorDependency;
            }

            object instance = null;
            if(constructorDependency != null)
            {
                var arguments = new object[constructorDependency.ArgumentDependencies.Count];
                for(int i = 0; i < constructorDependency.ArgumentDependencies.Count; i++)
                {
                    var dependency = constructorDependency.ArgumentDependencies[i];
                    try
                    {
                        arguments[i] = Resolve(dependency.Type, binding.TypeBoundTo, dependency.Category, dependency);
                    }
                    catch(Exception exception)
                    {
                        throw new Exception($"Could not resolve Type {dependency.Type.Name} in DefaultConstructor of Type {binding.TypeBoundTo.Name}.", exception);
                    }
                }
                instance = binding.Resolve(resolvingOnto, resolvingDependency, arguments);
            }
            else
            {
                instance = binding.Resolve(resolvingOnto, resolvingDependency);
            }

            if(instance == null)
            {
                throw new Exception($"Could not resolve {type.Name}.");
            }
            
            foreach(var dependency in dependencies)
            {
                switch(dependency.MemberType)
                {
                    case MemberTypes.Field:
                        var field = instance.GetType().GetField(dependency.MemberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                        field?.SetValue(instance, Resolve(dependency.Type, binding.TypeBoundTo, dependency.Category, dependency, instance));
                        break;
                    case MemberTypes.Property:
                        var property = instance.GetType().GetProperty(dependency.MemberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                        property?.SetValue(instance, Resolve(dependency.Type, binding.TypeBoundTo, dependency.Category, dependency, instance));
                        break;
                }
            }
            return instance;
        }
    }
    
    public abstract class Container : IDisposable
    {
        protected readonly Dictionary<Type, Dictionary<string, IBinding>> _bindings = new Dictionary<Type, Dictionary<string, IBinding>>();
        protected Binding _overrideBinding;
        
        protected Container() : this(null)
        {
        }
        
        protected Container(params Type[] inheritedContainers)
        {
            Context.AddContainer(this);
            if(inheritedContainers != null)
            {
                foreach(var inheritedContainerType in inheritedContainers)
                {
                    InheritFrom(inheritedContainerType);
                }
            }

            Setup();
        }

        private void Setup()
        {
            SetBindings();
            OnSetupComplete();
        }
        
        protected abstract void SetBindings();

        protected virtual void OnSetupComplete()
        {
        }
        
        public virtual IBinding Bind<T1, T2>(string category = "", T2 instance = null) where T2 : class, T1
        {
            var bindFromType = typeof(T1);
            if(!_bindings.TryGetValue(bindFromType, out var categoryBindingMap))
            {
                categoryBindingMap = new Dictionary<string, IBinding>();
                _bindings[bindFromType] = categoryBindingMap;
            }
            
            if(!categoryBindingMap.TryGetValue(category, out var binding))
            {
                binding = new Binding<T1, T2>(instance);
                categoryBindingMap[category] = binding;
            }
            else
            {
                throw new Exception($"Error: Type: {typeof(T1).Name} and Category: {category} already bound in Container: {GetType().Name}");
            }

            return binding;
        }
        
        protected void InheritFrom<T>() where T : Container
        {
            Context.AddInheritance<T>(this);
        }
        
        protected void InheritFrom(Type containerType)
        {
            Context.AddInheritance(this, containerType);
        }
        
        internal IBinding FindBinding(Type type, string category = "")
        {
            IBinding binding = null;
            if(!_bindings.TryGetValue(type, out var bindingMap) && !Context.FindInheritedBinding(this, type, category, out binding))
            {
                throw new Exception($"Error: Type: {type.Name} not bound in Container: {GetType().Name}");
            }

            if(binding == null && 
               bindingMap != null && 
               !bindingMap.TryGetValue(category, out binding) && 
               !Context.FindInheritedBinding(this, type, category, out binding))
            {
                throw new Exception($"Error: Type: {type.Name} and Category: {category} not bound in Container: {GetType().Name}");
            }

            return binding;
        }

        public T Resolve<T>(string category = "") where T : class
        {
            return Resolve(new Resolver(typeof(T), category, GetBinding, PreResolve)) as T;
        }

        protected virtual object Resolve(Resolver resolver)
        {
            return resolver.Resolve();
        }

        protected virtual void PreResolve(Type type, Type owner, string category, Dependency resolvingDependency = null, object resolvingOnto = null)
        {
        }

        protected virtual IBinding GetBinding(Type type, string category)
        {
            IBinding binding;
            try
            {
                binding = FindBinding(type, category);
                
                // TODO: Handle this better
                if(_overrideBinding != null)
                {
                    _overrideBinding.TypeBoundFrom = binding.TypeBoundFrom;
                    _overrideBinding.TypeBoundTo = binding.TypeBoundTo;
                    binding = _overrideBinding;
                }
            }
            catch
            {
                if(_overrideBinding != null)
                {
                    binding = _overrideBinding;
                }
                else
                {
                    throw;
                }
            }

            _overrideBinding = null;

            return binding;
        }
      
        public List<Tuple<string, IBinding>> GetBindings()
        {
            var list = new List<Tuple<string, IBinding>>();
            
            foreach(var bindingMaps in _bindings.Values)
            {
                foreach(var kvp in bindingMaps)
                {
                    list.Add(new Tuple<string, IBinding>(kvp.Key, kvp.Value));
                }
            }

            return list;
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!disposing)
            {
                return;
            }

            foreach(var bindings in _bindings.Values)
            {
                foreach(var binding in bindings.Values)
                {
                    binding.Dispose(); 
                }
                bindings.Clear();
            }
            _bindings.Clear();
        }

        public void Dispose()
        {
            Context.RemoveContainer(this);
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}