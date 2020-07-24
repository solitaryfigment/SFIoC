using UnityEngine;

namespace SF.IoC.Unity
{
    public class GameObjectFactory
    {
        [Inject] private Container _container = null;
        
        public T CreateGameObject<T>(Transform parent = null, string category = "") where T : Component
        {
            var resolved = _container.Resolve<T>(category);
            resolved.transform.SetParent(parent, false);
            return resolved;
        }
    }
}