using System;
using System.Collections.Generic;
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
    
    // TODO: make this work on static create methods
    [AttributeUsage(AttributeTargets.Constructor)]
    public class DefaultConstructorAttribute : Attribute
    {
        public DefaultConstructorAttribute()
        {
        }

        public Dependency CreateDependency(string memberName, MemberTypes memberType, List<Dependency> dependencies)
        {
            return new ConstructorDependency
            {
                MemberName = memberName,
                MemberType = memberType,
                ArgumentDependencies = dependencies
            };
        }
    }
}