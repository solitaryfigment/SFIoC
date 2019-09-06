using System;
using System.Reflection;

namespace SF.IoC
{
    public class Dependency
    {
        public MemberTypes MemberType;
        public string MemberName;
        public Type Type;
        public string Category = "";
    }
}