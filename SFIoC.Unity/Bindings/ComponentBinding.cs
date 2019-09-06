using UnityEngine;

namespace SF.IoC.Unity
{
    public class ComponentBinding : Binding
    {
        public ComponentBinding(System.Type typeBoundFrom, System.Type typeBoundTo) : base(typeBoundFrom, typeBoundTo)
        {
        }

        public override object Resolve(object resolvingOnto, Dependency dependency, params object[] args)
        {
            var componentDependency = dependency as ComponentReferenceDependency;
            var component = resolvingOnto as Component;
            if(componentDependency == null || component == null)
            {
                return base.Resolve(resolvingOnto, dependency, args);
            }
            
            if(!typeof(Object).IsAssignableFrom(TypeBoundTo))
            {
                throw new BindingException($"Type {TypeBoundTo.Name} must inherit from {nameof(Object)}.");
            }
            
            return component.transform.Find(componentDependency.ComponentReferencePath).GetComponent(TypeBoundFrom);
        }
    }
}