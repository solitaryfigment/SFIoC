using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace SF.IoC.Unity
{
    public abstract class UnityContainer : Container
    {
        protected UnityContainer() : base(null)
        {
        }

        protected UnityContainer(params Type[] inheritedContainers) : base(inheritedContainers)
        {
        }

        protected override void SetBindings()
        {
            Bind<Container, UnityContainer>("", this);
            Bind<GameObjectFactory, GameObjectFactory>().AsSingleton();
        }

        protected override void PreResolve(Type type, Type owner, string category, Dependency resolvingDependency = null, object resolvingOnto = null)
        {
            var componentDependency = resolvingDependency as ComponentReferenceDependency;
            if(componentDependency != null)
            {
                _overrideBinding = new ComponentBinding(componentDependency.Type, componentDependency.Type);
            }
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
                throw new System.Exception($"Error: Type: {nameof(T1)} and Category: {category} already bound in Container: {GetType().Name}");
            }

            return binding;
        }
    }
}