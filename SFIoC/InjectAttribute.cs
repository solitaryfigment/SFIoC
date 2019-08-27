using System;

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
    }
    [AttributeUsage(AttributeTargets.Parameter)]
    public class InjectArgumentAttribute : Attribute
    {
        public readonly string Category;

        public InjectArgumentAttribute(string category = "")
        {
            Category = category;
        }
    }
    
    [AttributeUsage(AttributeTargets.Constructor)]
    public class DefaultConstructorAttribute : Attribute
    {
        public DefaultConstructorAttribute()
        {
        }
    }
}