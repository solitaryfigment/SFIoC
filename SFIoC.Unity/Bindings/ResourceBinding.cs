using UnityEngine;

namespace SF.IoC.Unity
{
    public class ResourceBinding<T1, T2> : Binding<T1, T2> where T2 : UnityEngine.Object, T1
    {
        private readonly string _pathToPrefab;

        public ResourceBinding(string pathToPrefab, T2 instance) : base(instance)
        {
            if(!typeof(UnityEngine.Object).IsAssignableFrom(TypeBoundTo))
            {
                throw new BindingException($"Type {TypeBoundTo.Name} must inherit from {nameof(Component)}.");
            }

            _pathToPrefab = pathToPrefab;
        }

        public override object Resolve(object resolvingOnto, Dependency dependency, params object[] args)
        {
            if(_instance != null)
            {
                return _instance;
            }

            if(_bingingType == BingingType.Singleton)
            {
                _instance = (T1)Object.Instantiate(Resources.Load<T2>(_pathToPrefab));
            }

            return _instance ?? (T1)Object.Instantiate(Resources.Load<T2>(_pathToPrefab));
        }
    }
}
