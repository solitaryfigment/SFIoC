using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SF.IoC;

namespace SFIoC.Utils
{
    internal static class DependencyFinder
    {
        public static List<Dependency> GetDependencies<T>()
        {
            return GetDependencies(typeof(T));
        }

        public static List<Dependency> GetDependencies(object obj)
        {
            return GetDependencies(obj.GetType());
        }

        public static List<Dependency> GetDependencies(Type typeToSearch)
        {
            var dependencies = new List<Dependency>();
            var members = typeToSearch.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var injectedMembers = members.Where(m => m.GetCustomAttributes(true).Any(a => a is InjectAttribute));

            var constructorInfos = new List<ConstructorInfo>();
            constructorInfos.AddRange(typeToSearch.GetConstructors().Where(c => c.GetCustomAttributes(true).Any(a => a is DefaultConstructorAttribute)));
            if(constructorInfos.Count == 0)
            {
                var type = typeToSearch.BaseType;
                while(type!= null && type != typeof(object) && constructorInfos.Count == 0)
                {
                    constructorInfos.AddRange(type.GetConstructors().Where(c => c.GetCustomAttributes(true).Any(a => a is DefaultConstructorAttribute)));
                    type = type.BaseType;
                }
            }

            var constructorDependencies = new List<Dependency>();

            if(constructorInfos.Count == 1)
            {
                var constructorInfo = constructorInfos[0];
                var parameterInfos = constructorInfo.GetParameters();

                foreach(var parameterInfo in parameterInfos)
                {
                    var attribute = parameterInfo.GetCustomAttribute<InjectArgumentAttribute>(true);

                    if(attribute != null)
                    {
                        constructorDependencies.Add(attribute.CreateDependency(parameterInfo.Name, MemberTypes.Constructor, parameterInfo.ParameterType));
                    }
                    else
                    {
                        constructorDependencies.Add(new Dependency
                        {
                            MemberName = parameterInfo.Name,
                            MemberType = MemberTypes.Constructor,
                            Type = parameterInfo.ParameterType
                        });
                    }
                }

                if(constructorDependencies.Count > 0)
                {
                    var attribute = constructorInfo.GetCustomAttribute<DefaultConstructorAttribute>(true);
                    dependencies.Add(attribute.CreateDependency("Constructor", MemberTypes.Constructor, constructorDependencies));
                }
            }
            else if (constructorInfos.Count > 1)
            {
                throw new Exception($"Type {typeToSearch.Name} cannot contain 2 DefaultConstructors.");
            }

            foreach(var member in injectedMembers)
            {
                var property = member as PropertyInfo;
                var attribute = member.GetCustomAttribute<InjectAttribute>(true);
                if(!attribute.CanBeUsedOnType(typeToSearch))
                {
                    // TODO: Log error and continue
                    continue;
                }
                if(property != null)
                {
                    dependencies.Add(attribute.CreateDependency(property.Name, MemberTypes.Property, property.PropertyType));
                }
                else
                {
                    var field = member as FieldInfo;
                    if(field != null)
                    {
                        dependencies.Add(attribute.CreateDependency(field.Name, MemberTypes.Field, field.FieldType));
                    }
                }
            }

            return dependencies;
        }
    }
}
