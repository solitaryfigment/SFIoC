using System;
using System.Collections.Generic;
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
    
    public class ConstructorDependency : Dependency
    {
        public List<Dependency> ArgumentDependencies = new List<Dependency>();
    }
}