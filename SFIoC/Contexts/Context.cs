using System;
using System.Collections.Generic;

namespace SF.IoC
{
    public static class Context
    {
        private static readonly Dictionary<Type, List<Type>> _containerInheritanceMap = new Dictionary<Type, List<Type>>();
        private static readonly Dictionary<Type, Container> _typeToContainerMap = new Dictionary<Type, Container>();
        private static bool _disposing;
        
        static Context()
        {
            // TODO: Find App Context Type
        }
        
        internal static void AddContainer(Container container)
        {
            var containerType = container.GetType();
            if(_typeToContainerMap.TryGetValue(containerType, out var boundContainer))
            {
                throw new Exception($"Error: Container Name: {container.GetType().Name} is already bound to Type: {boundContainer.GetType().Name}. Cannot rebind to Type: {containerType.Name}");
            }

            _typeToContainerMap[containerType] = container;
        }

        internal static void RemoveContainer(Container container)
        {
            if(_disposing)
            {
                Console.WriteLine(_disposing);
                return;
            }
            var containerType = container.GetType();
            
            if(!_typeToContainerMap.Remove(containerType))
            {
                Console.WriteLine("Type not bound");
                return;
            }
        }

        internal static void AddInheritance<T>(Container container) where T : Container
        {
            AddInheritance(container, typeof(T));
        }

        internal static void AddInheritance(Container container, Type inheritedContainerType)
        {
            var containerType = container.GetType();
            if(!_containerInheritanceMap.TryGetValue(containerType, out var inheritanceList))
            {
                inheritanceList = new List<Type>();
                _containerInheritanceMap[containerType] = inheritanceList;
            }

            if(!inheritanceList.Contains(inheritedContainerType))
            {
                inheritanceList.Add(inheritedContainerType);
            }
        }

        internal static bool FindInheritedBinding(Container container, Type type, string category, out IBinding binding)
        {
            var containerType = container.GetType();
            binding = null;
            if(!_containerInheritanceMap.TryGetValue(containerType, out var inheritedContainerNames))
            {
                return false;
            }

            foreach(var inheritedContainerName in inheritedContainerNames)
            {
                try
                {
                    var inheritedContainer = GetContainerByType(inheritedContainerName);
                    binding = inheritedContainer.FindBinding(type, category);
                    if(binding != null)
                    {
                        Console.WriteLine("binding not null in " + inheritedContainer.GetType().Name);
                        return true;
                    }
                    Console.WriteLine("binding null in " + inheritedContainer.GetType().Name);
                }
                catch
                {
                    continue;
                }
            }

            return false;
        }
        
        public static Container GetContainerByType<T>() where T : Container
        {
            if(!_typeToContainerMap.TryGetValue(typeof(T), out var container))
            {
                throw new Exception($"Error: No Container bound for Type: {nameof(T)}");
            }

            return container;
        }

        public static Container GetContainerByType(Type containerType)
        {
            if(!_typeToContainerMap.TryGetValue(containerType, out var container))
            {
                throw new Exception($"Error: No Container bound for Type: {containerType.Name}");
            }

            return container;
        }
        
        public static void Dispose()
        {
            _disposing = true;
            _containerInheritanceMap.Clear();
            foreach(var container in _typeToContainerMap.Values)
            {
                container.Dispose();
            }
            _typeToContainerMap.Clear();
            _disposing = false;
        }
    }
}