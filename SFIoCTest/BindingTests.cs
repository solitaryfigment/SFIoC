using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using SF.IoC;

namespace SFIoCTest
{
    [TestFixture]
    public class BindingTests
    {
        [Test]
        public void MustInheritFromType()
        {
            var binding1 = new Binding<Interface, ConcreteInterface>();
            var binding2 = new Binding<AbstractClass, ConcreteAbstractClass>();
            Assert.Throws<BindingException>(() => { new Binding<SubClass, BaseClass>(); });
        }

        [Test]
        public void CanBindToInstance()
        {
            var obj = new ConcreteInterface();
            var binding1 = new Binding<Interface, ConcreteInterface>(obj);
            var resolvedObj = binding1.Resolve();
            
            Assert.NotNull(resolvedObj);
            Assert.NotNull(obj);
        }

        [Test]
        public void TransientBindingResolvesNewObject()
        {
            var binding1 = new Binding<Interface, ConcreteInterface>();
            var resolvedObj1 = binding1.Resolve();
            var resolvedObj2 = binding1.Resolve();
            
            Assert.NotNull(resolvedObj1);
            Assert.NotNull(resolvedObj2);
            Assert.AreNotEqual(resolvedObj1, resolvedObj2);
        }

        [Test]
        public void InstancedBindingResolvesSameObject()
        {
            var obj = new ConcreteInterface();
            var binding1 = new Binding<Interface, ConcreteInterface>(obj);
            var resolvedObj1 = binding1.Resolve();
            var resolvedObj2 = binding1.Resolve();
            
            Assert.NotNull(resolvedObj1);
            Assert.NotNull(resolvedObj2);
            Assert.AreEqual(obj, resolvedObj2);
            Assert.AreEqual(obj, resolvedObj2);
            Assert.AreEqual(resolvedObj1, resolvedObj2);
        }

        [Test]
        public void SingletonBindingResolvesSameObject()
        {
            var binding1 = new Binding<Interface, ConcreteInterface>().AsSingleton();
            var resolvedObj1 = binding1.Resolve();
            var resolvedObj2 = binding1.Resolve();
            
            Assert.NotNull(resolvedObj1);
            Assert.NotNull(resolvedObj2);
            Assert.AreEqual(resolvedObj1, resolvedObj2);
        }

        [Test]
        public void CanResolveWithParams()
        {
            var binding1 = new Binding<BaseClass, SubClassWithConstructorArgs>();
            var resolved = binding1.Resolve(42, 4.2f) as SubClassWithConstructorArgs;

            Assert.NotNull(resolved);
            Assert.AreEqual(42, resolved.I);
            Assert.AreEqual(4.2f, resolved.F);
        }

        [Test]
        public void TransientBindingResolveWithParamsAreDifferent()
        {
            var binding1 = new Binding<BaseClass, SubClassWithConstructorArgs>();
            var resolvedObj1 = binding1.Resolve(42, 4.2f) as SubClassWithConstructorArgs;
            var resolvedObj2 = binding1.Resolve(3, 1.1f) as SubClassWithConstructorArgs;
            
            Assert.NotNull(resolvedObj1);
            Assert.NotNull(resolvedObj2);
            Assert.AreNotEqual(resolvedObj1, resolvedObj2);
            Assert.AreEqual(42, resolvedObj1.I);
            Assert.AreEqual(4.2f, resolvedObj1.F);
            Assert.AreEqual(3, resolvedObj2.I);
            Assert.AreEqual(1.1f, resolvedObj2.F);
        }

        [Test]
        public void SingletonBindingResolveWithParamsAreSameAndIgnoreArgs()
        {
            var binding1 = new Binding<BaseClass, SubClassWithConstructorArgs>().AsSingleton();
            var resolvedObj1 = binding1.Resolve(42, 4.2f) as SubClassWithConstructorArgs;
            var resolvedObj2 = binding1.Resolve(3, 1.1f) as SubClassWithConstructorArgs;
            
            Assert.NotNull(resolvedObj1);
            Assert.NotNull(resolvedObj2);
            Assert.AreEqual(resolvedObj1, resolvedObj2);
            Assert.AreEqual(42, resolvedObj1.I);
            Assert.AreEqual(4.2f, resolvedObj1.F);
            Assert.AreEqual(42, resolvedObj2.I);
            Assert.AreEqual(4.2f, resolvedObj2.F);
        }

        [Test]
        public void InstancedBindingResolveWithParamsAreSameAndIgnoreArgs()
        {
            var instance = new SubClassWithConstructorArgs(90, 0.9f);
            var binding1 = new Binding<BaseClass, SubClassWithConstructorArgs>(instance);
            var resolvedObj1 = binding1.Resolve(42, 4.2f) as SubClassWithConstructorArgs;
            var resolvedObj2 = binding1.Resolve(3, 1.1f) as SubClassWithConstructorArgs;
            
            Assert.NotNull(resolvedObj1);
            Assert.NotNull(resolvedObj2);
            Assert.AreEqual(instance, resolvedObj1);
            Assert.AreEqual(instance, resolvedObj2);
            Assert.AreEqual(resolvedObj1, resolvedObj2);
            Assert.AreEqual(90, resolvedObj1.I);
            Assert.AreEqual(0.9f, resolvedObj1.F);
            Assert.AreEqual(90, resolvedObj2.I);
            Assert.AreEqual(0.9f, resolvedObj2.F);
        }

        [Test]
        public void DisposedTransientBindingsCleanBindingProperly()
        {
            var binding1 = new Binding<BaseClass, SubClassWithFieldDependencies>();
            var dependencies = binding1.GetDependencies();
            var dependenciesField = typeof(Binding<BaseClass,SubClassWithFieldDependencies>).GetField("_dependencies", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(dependenciesField);
            var privateDependencies = dependenciesField.GetValue(binding1) as List<Dependency>;
            
            Assert.NotNull(dependencies);
            Assert.NotNull(privateDependencies);
            Assert.AreEqual(2, dependencies.Count);
            Assert.AreEqual(2, privateDependencies.Count);
            
            binding1.Dispose();
            var privateDependenciesAfter = dependenciesField.GetValue(binding1) as List<Dependency>;
            Assert.AreEqual(0, privateDependenciesAfter.Count);
            Assert.IsFalse(binding1.HasInstanceAvailable());
        }

        [Test]
        public void DisposedSingletonBindingsCleanBindingProperly()
        {
            var binding1 = new Binding<BaseClass, SubClassWithFieldDependencies>().AsSingleton();
            var dependencies = binding1.GetDependencies();
            var dependenciesField = typeof(Binding<BaseClass,SubClassWithFieldDependencies>).GetField("_dependencies", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(dependenciesField);
            var privateDependencies = dependenciesField.GetValue(binding1) as List<Dependency>;
            
            Assert.NotNull(dependencies);
            Assert.NotNull(privateDependencies);
            Assert.AreEqual(2, dependencies.Count);
            Assert.AreEqual(2, privateDependencies.Count);
            
            binding1.Dispose();
            var privateDependenciesAfter = dependenciesField.GetValue(binding1) as List<Dependency>;
            Assert.AreEqual(0, privateDependenciesAfter.Count);
            Assert.IsFalse(binding1.HasInstanceAvailable());
        }

        [Test]
        public void DisposedInstanceBindingsCleanBindingProperly()
        {
            var binding1 = new Binding<BaseClass, SubClassWithFieldDependencies>(new SubClassWithFieldDependencies());
            var dependencies = binding1.GetDependencies();
            var dependenciesField = typeof(Binding<BaseClass,SubClassWithFieldDependencies>).GetField("_dependencies", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(dependenciesField);
            var privateDependencies = dependenciesField.GetValue(binding1) as List<Dependency>;
            
            Assert.NotNull(dependencies);
            Assert.NotNull(privateDependencies);
            Assert.AreEqual(2, dependencies.Count);
            Assert.AreEqual(2, privateDependencies.Count);
            
            binding1.Dispose();
            var privateDependenciesAfter = dependenciesField.GetValue(binding1) as List<Dependency>;
            Assert.AreEqual(0, privateDependenciesAfter.Count);
            Assert.IsFalse(binding1.HasInstanceAvailable());
        }

        [Test]
        public void TransientBindingDoesNotHaveInstanceAvailable()
        {
            var binding1 = new Binding<BaseClass, SubClassWithFieldDependencies>();
            
            Assert.IsFalse(binding1.HasInstanceAvailable());
            var resolved = binding1.Resolve();
            Assert.NotNull(resolved);
            Assert.IsFalse(binding1.HasInstanceAvailable());
        }

        [Test]
        public void SingletonBindingHasInstanceAvailable()
        {
            var binding1 = new Binding<BaseClass, SubClassWithFieldDependencies>().AsSingleton();
            
            Assert.IsFalse(binding1.HasInstanceAvailable());
            var resolved = binding1.Resolve();
            Assert.NotNull(resolved);
            Assert.IsTrue(binding1.HasInstanceAvailable());
        }

        [Test]
        public void InstancedBindingHasInstanceAvailable()
        {
            var binding1 = new Binding<BaseClass, SubClassWithFieldDependencies>(new SubClassWithFieldDependencies());
            
            Assert.IsTrue(binding1.HasInstanceAvailable());
            var resolved = binding1.Resolve();
            Assert.NotNull(resolved);
            Assert.IsTrue(binding1.HasInstanceAvailable());
        }

        [Test]
        public void CanGetFieldDependencies()
        {
            var binding1 = new Binding<BaseClass, SubClassWithFieldDependencies>();
            var dependencies = binding1.GetDependencies();

            Assert.NotNull(dependencies);
            Assert.AreEqual(2, dependencies.Count);

            Assert.AreEqual("Interface", dependencies[0].MemberName);
            Assert.AreEqual(typeof(Interface), dependencies[0].Type);
            Assert.AreEqual(MemberTypes.Field, dependencies[0].MemberType);
            Assert.AreEqual("", dependencies[0].Category);

            Assert.AreEqual("InterfaceWithCategory", dependencies[1].MemberName);
            Assert.AreEqual(typeof(Interface), dependencies[1].Type);
            Assert.AreEqual(MemberTypes.Field, dependencies[1].MemberType);
            Assert.AreEqual("First", dependencies[1].Category);
        }

        [Test]
        public void CanGetPropertyDependencies()
        {
            var binding1 = new Binding<BaseClass, SubClassWithPropetyDependencies>();
            var dependencies = binding1.GetDependencies();

            Assert.NotNull(dependencies);
            Assert.AreEqual(2, dependencies.Count);

            Assert.AreEqual("Interface", dependencies[0].MemberName);
            Assert.AreEqual(typeof(Interface), dependencies[0].Type);
            Assert.AreEqual(MemberTypes.Property, dependencies[0].MemberType);
            Assert.AreEqual("", dependencies[0].Category);

            Assert.AreEqual("InterfaceWithCategory", dependencies[1].MemberName);
            Assert.AreEqual(typeof(Interface), dependencies[1].Type);
            Assert.AreEqual(MemberTypes.Property, dependencies[1].MemberType);
            Assert.AreEqual("First", dependencies[1].Category);
        }

        [Test]
        public void CanGetConstructorDependencies()
        {
            var binding1 = new Binding<BaseClass, SubClassWithConstructorDependencies>();
            var dependencies = binding1.GetDependencies();

            Assert.NotNull(dependencies);
            Assert.AreEqual(1, dependencies.Count);
            Assert.AreEqual("Constructor", dependencies[0].MemberName);
            Assert.AreEqual(MemberTypes.Constructor, dependencies[0].MemberType);
            
            var constructorDependencies = ((ConstructorDependency)dependencies[0]).ArgumentDependencies;

            Assert.NotNull(constructorDependencies);
            Assert.AreEqual(2, constructorDependencies.Count);

            Assert.AreEqual("interface", constructorDependencies[0].MemberName);
            Assert.AreEqual(typeof(Interface), constructorDependencies[0].Type);
            Assert.AreEqual(MemberTypes.Constructor, constructorDependencies[0].MemberType);
            Assert.AreEqual("", constructorDependencies[0].Category);

            Assert.AreEqual("abstractClass", constructorDependencies[1].MemberName);
            Assert.AreEqual(typeof(AbstractClass), constructorDependencies[1].Type);
            Assert.AreEqual(MemberTypes.Constructor, constructorDependencies[1].MemberType);
            Assert.AreEqual("First", constructorDependencies[1].Category);
        }

        [Test]
        public void CanGetConstructorDependenciesFromInheritedClass()
        {
            var binding1 = new Binding<BaseClass, SubSubClassWithConstructorDependencies>();
            var dependencies = binding1.GetDependencies();

            Assert.NotNull(dependencies);
            Assert.AreEqual(1, dependencies.Count);
            Assert.AreEqual("Constructor", dependencies[0].MemberName);
            Assert.AreEqual(MemberTypes.Constructor, dependencies[0].MemberType);
            
            var constructorDependencies = ((ConstructorDependency)dependencies[0]).ArgumentDependencies;

            Assert.NotNull(constructorDependencies);
            Assert.AreEqual(2, constructorDependencies.Count);

            Assert.AreEqual("interface", constructorDependencies[0].MemberName);
            Assert.AreEqual(typeof(Interface), constructorDependencies[0].Type);
            Assert.AreEqual(MemberTypes.Constructor, constructorDependencies[0].MemberType);
            Assert.AreEqual("", constructorDependencies[0].Category);

            Assert.AreEqual("abstractClass", constructorDependencies[1].MemberName);
            Assert.AreEqual(typeof(AbstractClass), constructorDependencies[1].Type);
            Assert.AreEqual(MemberTypes.Constructor, constructorDependencies[1].MemberType);
            Assert.AreEqual("First", constructorDependencies[1].Category);
        }
    }
}
