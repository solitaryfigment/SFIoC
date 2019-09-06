using System.Collections.Generic;
using UnityEngine;

namespace SF.IoC.Unity
{
    public abstract class UnityContainer : Container
    {
        protected UnityContainer(string name) : base(name, null)
        {
        }

        protected UnityContainer(string name, params string[] inheritedContainers) : base(name, inheritedContainers)
        {
        }

        protected override object Resolve(System.Type type, System.Type owner, Dictionary<System.Type, List<IBinding>> resolvedBindings, string category, Dependency resolvingDependency = null, object resolvingOnto = null)
        {
            var componentDependency = resolvingDependency as ComponentReferenceDependency;
            if(componentDependency != null)
            {
                _overrideBinding = new ComponentBinding(componentDependency.Type, componentDependency.Type);
            }
            return base.Resolve(type, owner, resolvedBindings, category, resolvingDependency, resolvingOnto);
        }
        
        public IBinding BindPrefab<T1, T2>(string pathToPrefab, string category = "", T2 instance = null) where T2 : Object, T1
        {
            var bindFromType = typeof(T1);
            if(!_bindings.TryGetValue(bindFromType, out var categoryBindingMap))
            {
                categoryBindingMap = new Dictionary<string, IBinding>();
                _bindings[bindFromType] = categoryBindingMap;
            }
            
            if(!categoryBindingMap.TryGetValue(category, out var binding))
            {
                binding = new PrefabBinding<T1, T2>(pathToPrefab, instance);
                categoryBindingMap[category] = binding;
            }
            else
            {
                throw new System.Exception($"Error: Type: {nameof(T1)} and Category: {category} already bound in Container: {Name}");
            }

            return binding;
        }
    }
}