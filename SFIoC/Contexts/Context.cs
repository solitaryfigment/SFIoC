using System;
using System.Collections.Generic;

namespace SF.IoC
{
    public static class Context
    {
        private static readonly Dictionary<string, Type> _nameToContainerTypeMap = new Dictionary<string, Type>();
        private static readonly Dictionary<string, List<string>> _containerInheritanceMap = new Dictionary<string, List<string>>();
        private static readonly Dictionary<Type, Dictionary<string, Container>> _typeToContainerMap = new Dictionary<Type, Dictionary<string, Container>>();
        private static bool _disposing;
        
        static Context()
        {
            // TODO: Find App Context Type
        }
        
        internal static void AddContainer(Container container)
        {
            var containerType = container.GetType();
            if(_nameToContainerTypeMap.TryGetValue(container.Name, out var type))
            {
                throw new Exception($"Error: Container Name: {container.Name} is already bound to Type: {type.Name}. Cannot rebind to Type: {containerType.Name}");
            }
            _nameToContainerTypeMap[container.Name] = containerType;
            
            if(!_typeToContainerMap.TryGetValue(containerType, out var containersBoundToType))
            {
                containersBoundToType = new Dictionary<string, Container>();
                _typeToContainerMap[containerType] = containersBoundToType;
            }
            
            // Just in case, this should not happen and should be caught by the _nameToContainerTypeMap check
            if(containersBoundToType.TryGetValue(container.Name, out var boundContainer))
            {
                throw new Exception($"Error: Container Name: {container.Name} is already bound to Type: {boundContainer.GetType().Name}. Cannot rebind to Type: {containerType.Name}");
            }

            containersBoundToType[container.Name] = container;
        }

        internal static void RemoveContainer(Container container)
        {
            if(_disposing)
            {
                Console.WriteLine(_disposing);
                return;
            }
            var containerType = container.GetType();
            if(!_nameToContainerTypeMap.ContainsKey(container.Name))
            {
                Console.WriteLine("Name not bound");
                return;
            }
            _nameToContainerTypeMap.Remove(container.Name);
            _containerInheritanceMap.Remove(container.Name);
            
            if(!_typeToContainerMap.TryGetValue(containerType, out var containersBoundToType))
            {
                Console.WriteLine("Type not bound");
                return;
            }
            
            if(containersBoundToType.ContainsKey(container.Name))
            {
                Console.WriteLine("Removed");
                containersBoundToType.Remove(container.Name);
            }
            Console.WriteLine("Could not find");
        }

        internal static void AddInheritance(string containerName, string containerNameToInherit)
        {
            if(!_containerInheritanceMap.TryGetValue(containerName, out var inheritanceList))
            {
                inheritanceList = new List<string>();
                _containerInheritanceMap[containerName] = inheritanceList;
            }

            if(!inheritanceList.Contains(containerNameToInherit))
            {
                inheritanceList.Add(containerNameToInherit);
            }
        }

        internal static bool FindInheritedBinding(string containerName, Type type, string category, out IBinding binding)
        {
            binding = null;
            if(!_containerInheritanceMap.TryGetValue(containerName, out var inheritedContainerNames))
            {
                return false;
            }

            foreach(var inheritedContainerName in inheritedContainerNames)
            {
                try
                {
                    var container = GetContainerByName(inheritedContainerName);
                    binding = container.FindBinding(type, category);
                    if(binding != null)
                    {
                        Console.WriteLine("binding not null in " + container.Name);
                        return true;
                    }
                    Console.WriteLine("binding null in " + container.Name);
                }
                catch
                {
                    continue;
                }
            }

            return false;
        }
        
        public static Container GetContainerByType<T>(string containerName) where T : Container
        {
            if(!_typeToContainerMap.TryGetValue(typeof(T), out var containersBoundToType))
            {
                throw new Exception($"Error: No Container bound for Type: {nameof(T)}");
            }
            if(!containersBoundToType.TryGetValue(containerName, out var container))
            {
                throw new Exception($"Error: No Container bound for Type: {nameof(T)} and Name: {containerName}.");
            }

            return container;
        }

        public static Container GetContainerByType(Type containerType, string containerName)
        {
            if(!_typeToContainerMap.TryGetValue(containerType, out var containersBoundToType))
            {
                throw new Exception($"Error: No Container bound for Type: {containerType.Name}");
            }
            if(!containersBoundToType.TryGetValue(containerName, out var container))
            {
                throw new Exception($"Error: No Container bound for Type: {containerType.Name} and Name: {containerName}.");
            }

            return container;
        }

        public static Container GetContainerByName(string containerName)
        {
            if(!_nameToContainerTypeMap.TryGetValue(containerName, out var containerType))
            {
                Console.WriteLine("Not Found Parent");
                throw new Exception($"Error: No Container bound to Name: {containerName}");
            }
            if(!_typeToContainerMap.TryGetValue(containerType, out var containersBoundToType))
            {
                Console.WriteLine("Not Found Parent");
                throw new Exception($"Error: No Container bound for Type: {containerType.Name}.");
            }
            if(!containersBoundToType.TryGetValue(containerName, out var container))
            {
                Console.WriteLine("Not Found Parent");
                throw new Exception($"Error: No Container bound for Type: {containerType.Name} and Name: {containerName}.");
            }

            Console.WriteLine("Found Parent");
            return container;
        }

        public static void Dispose()
        {
            _disposing = true;
            _nameToContainerTypeMap.Clear();
            _containerInheritanceMap.Clear();
            foreach(var kvp in _typeToContainerMap)
            {
                foreach(var container in kvp.Value.Values)
                {
                    container.Dispose();
                }
            }
            _typeToContainerMap.Clear();
            _disposing = false;
        }
    }
}