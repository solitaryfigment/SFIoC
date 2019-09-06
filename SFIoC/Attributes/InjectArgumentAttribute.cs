using System;
using System.Reflection;

namespace SF.IoC
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class InjectArgumentAttribute : Attribute
    {
        public readonly string Category;

        public InjectArgumentAttribute(string category = "")
        {
            Category = category;
        }

        public Dependency CreateDependency(string memberName, MemberTypes memberType, Type type)
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