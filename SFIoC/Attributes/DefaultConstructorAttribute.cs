using System;
using System.Collections.Generic;
using System.Reflection;

namespace SF.IoC
{
    // TODO: Make this work with static Factory Methods
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