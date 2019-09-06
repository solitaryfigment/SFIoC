using System;
using System.Reflection;

namespace SF.IoC
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class InjectAttribute : Attribute
    {
        public readonly string Category;

        public InjectAttribute(string category = "")
        {
            Category = category;
        }

        public virtual bool CanBeUsedOnType(Type type)
        {
            return true;
        }

        public virtual Dependency CreateDependency(string memberName, MemberTypes memberType, Type type)
        {
            return new Dependency
            {
                MemberName = memberName,
                MemberType = memberType,
                Type = type,
                Category = Category
            };
        }
    }
}