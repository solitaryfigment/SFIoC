using System.Collections.Generic;

namespace SF.IoC
{
    public class ConstructorDependency : Dependency
    {
        public List<Dependency> ArgumentDependencies = new List<Dependency>();
    }
}