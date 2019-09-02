using System;
using System.Collections.Generic;
using System.Reflection;

namespace SF.IoC
{
    public abstract class Container : IDisposable
    {
        protected readonly Dictionary<Type, Dictionary<string, IBinding>> _bindings = new Dictionary<Type, Dictionary<string, IBinding>>();
        protected Binding _tempBinding;
        
        public string Name { get; private set; }
        
        protected Container(string name) : this(name, null)
        {
        }
        
        protected Container(string name, params string[] inheritedContainers)
        {
            Name = name;
            Context.AddContainer(this);
            if(inheritedContainers == null)
            {
                return;
            }
            foreach(var inheritedContainerName in inheritedContainers)
            {
                InheritFrom(inheritedContainerName);
            }
        }
        
        public virtual IBinding Bind<T1, T2>(string category = "", T2 instance = null) where T1 : class where T2 : class
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
                throw new Exception($"Error: Type: {typeof(T1).Name} and Category: {category} already bound in Container: {Name}");
            }

            return binding;
        }
        
        protected void InheritFrom(string containerNameToInherit)
        {
            Context.AddInheritance(Name, containerNameToInherit);
        }
        
        internal IBinding FindBinding(Type type, string category = "")
        {
            IBinding binding = null;
            if(!_bindings.TryGetValue(type, out var bindingMap) && !Context.FindInheritedBinding(Name, type, category, out binding))
            {
                throw new Exception($"Error: Type: {type.Name} not bound in Container: {Name}");
            }

            if(binding == null && 
               bindingMap != null && 
               !bindingMap.TryGetValue(category, out binding) && 
               !Context.FindInheritedBinding(Name, type, category, out binding))
            {
                throw new Exception($"Error: Type: {type.Name} and Category: {category} not bound in Container: {Name}");
            }

            return binding;
        }

        public T Resolve<T>(string category = "") where T : class
        {
            return Resolve(typeof(T), null, new Dictionary<Type, List<IBinding>>(), category) as T;
        }

        protected IBinding GetBinding(Type type, string category)
        {
            IBinding binding;
            try
            {
                binding = FindBinding(type, category);
                if(_tempBinding != null)
                {
                    _tempBinding.TypeBoundFrom = binding.TypeBoundFrom;
                    _tempBinding.TypeBoundTo = binding.TypeBoundTo;
                    binding = _tempBinding;
                }
            }
            catch(Exception e)
            {
                if(_tempBinding != null)
                {
                    binding = _tempBinding;
                }
                else
                {
                    throw;
                }
            }

            _tempBinding = null;

            return binding;
        }
        protected virtual object Resolve(Type type, Type owner, Dictionary<Type, List<IBinding>> resolvedBindings, string category, Dependency resolvingDependency = null, object resolvingOnto = null)
        {
            var binding = GetBinding(type, category);
            if(binding.HasInstanceAvailable())
            {
                return binding.Resolve(resolvingOnto, resolvingDependency);
            }
            
            List<IBinding> bindings = null;
            if(owner != null && !resolvedBindings.TryGetValue(owner, out bindings))
            {
                bindings = new List<IBinding>();
                resolvedBindings.Add(owner, bindings);
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
                        arguments[i] = Resolve(dependency.Type, binding.TypeBoundTo, resolvedBindings, dependency.Category, dependency);
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
                        field?.SetValue(instance, Resolve(dependency.Type, binding.TypeBoundTo, resolvedBindings, dependency.Category, dependency, instance));
                        break;
                    case MemberTypes.Property:
                        var property = instance.GetType().GetProperty(dependency.MemberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                        property?.SetValue(instance, Resolve(dependency.Type, binding.TypeBoundTo, resolvedBindings, dependency.Category, dependency, instance));
                        break;
                }
            }
            return instance;
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