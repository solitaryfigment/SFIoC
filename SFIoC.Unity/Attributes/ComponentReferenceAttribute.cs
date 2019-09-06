using System.Reflection;
using UnityEngine;

namespace SF.IoC.Unity
{
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
    public class ComponentReferenceAttribute : InjectAttribute
    {
        private static readonly System.Type _monoBehaviourType = typeof(MonoBehaviour);
        private readonly string _pathToChildReference; 

        public ComponentReferenceAttribute(string pathToChildReference = "", string category = "") : base(category)
        {
            _pathToChildReference = pathToChildReference;
        }

        public override bool CanBeUsedOnType(System.Type type)
        {
            if(_monoBehaviourType.IsAssignableFrom(type))
            {
                return true;
            }

            Debug.LogError($"{nameof(ComponentReferenceAttribute)} can only be used on MonoBehaviours.");
            return false;
        }

        public override Dependency CreateDependency(string memberName, MemberTypes memberType, System.Type type)
        {
            return new ComponentReferenceDependency
            {
                MemberName = memberName,
                MemberType = memberType,
                Type = type,
                Category = Category,
                ComponentReferencePath = _pathToChildReference
            };
        }
    }
}