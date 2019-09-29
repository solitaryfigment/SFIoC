using System;
using NUnit.Framework;
using SF.IoC;

namespace SFIoCTest
{
    [TestFixture]
    public class InheritedContainerTests
    {
        private Container _container;
        private Container _parentContainer;
        
        [SetUp]
        public void Setup()
        {
            Context.Dispose();
            _parentContainer = new NameableTestContainer();
            _parentContainer.Bind<Interface, ConcreteInterface>().AsSingleton();
            _parentContainer.Bind<AbstractClass, ConcreteAbstractClass>();
            _parentContainer.Bind<BaseClass, SubClassWithFieldDependencies>().AsSingleton();
            _parentContainer.Bind<BaseClass, SubClassWithCircularDependencies>("Circle");
            _parentContainer.Bind<BaseClass, OtherSubClassWithCircularDependencies>("Other");
            _parentContainer.Bind<Interface, ConcreteInterface>("First");
            _parentContainer.Bind<AbstractClass, ConcreteAbstractClass>("Second");
            _parentContainer.Bind<BaseClass, SubClassWithCategory>("Third");
            
            _container = new InheritTestContainer(typeof(NameableTestContainer));
            _container.Bind<Interface, ConcreteInterface>().AsSingleton();
            _container.Bind<BaseClass, SubClassWithCircularDependencies>("Circle");
            _container.Bind<BaseClass, OtherSubClassWithCircularDependencies>("Other");
            _container.Bind<Interface, ConcreteInterface>("First");
            _container.Bind<AbstractClass, ConcreteAbstractClass>("Second");
            _container.Bind<BaseClass, SubClassWithCategory>("Third");
        }
        
        [Test]
        public void CanResolveInheritedTransient()
        {
            var obj = _container.Resolve<AbstractClass>();
            var objCategory = _container.Resolve<AbstractClass>("Second");
            Assert.NotNull(obj);
            Assert.NotNull(objCategory);
            Assert.AreEqual(typeof(ConcreteAbstractClass),obj.GetType());
            Assert.AreEqual(typeof(ConcreteAbstractClass),objCategory.GetType());
            Assert.AreNotEqual(obj, objCategory);
        }
        
        [Test]
        public void CanResolveInheritedSingleton()
        {
            var obj = _container.Resolve<BaseClass>();
            var objCategory = _container.Resolve<BaseClass>("Third");
            Assert.NotNull(obj);
            Assert.NotNull(objCategory);
            Assert.AreEqual(typeof(SubClassWithFieldDependencies),obj.GetType());
            Assert.AreEqual(typeof(SubClassWithCategory),objCategory.GetType());
            Assert.AreNotEqual(obj, objCategory);
        }
        
        [Test]
        public void CanResolveInheritedInstanced()
        {
            var obj = new ConcreteInterface();
            _container.Bind<Interface, ConcreteInterface>("Instanced", obj).AsTransient();
            var resolved = _container.Resolve<Interface>("Instanced");
            var nonInstanced = _container.Resolve<Interface>();
            Assert.NotNull(obj);
            Assert.NotNull(resolved);
            Assert.AreEqual(typeof(ConcreteInterface),obj.GetType());
            Assert.AreEqual(typeof(ConcreteInterface),resolved.GetType());
            Assert.AreEqual(obj.GetHashCode(),resolved.GetHashCode());
            Assert.AreNotEqual(obj,nonInstanced);
        }
        
        [Test]
        public void SingletonTypesBoundInBothContainerAndInheritedContainerAreNotEqual()
        {
            var obj = _container.Resolve<Interface>();
            var objInherited = _parentContainer.Resolve<Interface>();
            Assert.NotNull(obj);
            Assert.NotNull(objInherited);
            Assert.AreNotEqual(objInherited,obj);
        }
        
        [Test]
        public void SingletonTypesBoundOnlyInInheritedContainerAreEqual()
        {
            var obj = _container.Resolve<BaseClass>();
            var objInherited = _parentContainer.Resolve<BaseClass>();
            Assert.NotNull(obj);
            Assert.NotNull(objInherited);
            Assert.AreEqual(objInherited,obj);
        }

        [Test]
        public void CanInheritMultipleContainers()
        {
            var parent1Container = new ExtraContainer1();
            parent1Container.Bind<Interface, ConcreteInterface>().AsSingleton();
            parent1Container.Bind<AbstractClass, ConcreteAbstractClass>().AsSingleton();
            
            var parent2Container = new ExtraContainer2();
            parent2Container.Bind<Interface, ConcreteInterface>().AsSingleton();
            parent2Container.Bind<AbstractClass, ConcreteAbstractClass>().AsSingleton();
            parent2Container.Bind<BaseClass, SubClass>().AsSingleton();

            var container = new ExtraContainer3(typeof(ExtraContainer1), typeof(ExtraContainer2));
            container.Bind<Interface, ConcreteInterface>().AsSingleton();

            var baseClass = container.Resolve<BaseClass>();
            var parent2BaseClass = parent2Container.Resolve<BaseClass>();
            var abstractClass = container.Resolve<AbstractClass>();
            var parent1AbstractClass = parent1Container.Resolve<AbstractClass>();
            var parent2AbstractClass = parent2Container.Resolve<AbstractClass>();
            var parent1Interface = parent1Container.Resolve<Interface>();
            var parent2Interface = parent2Container.Resolve<Interface>();
            var childInterface = container.Resolve<Interface>();
            
            Assert.NotNull(baseClass);
            Assert.NotNull(parent2BaseClass);
            Assert.NotNull(abstractClass);
            Assert.NotNull(parent1AbstractClass);
            Assert.NotNull(parent2AbstractClass);
            Assert.NotNull(parent1Interface);
            Assert.NotNull(parent2Interface);
            Assert.NotNull(childInterface);
            
            Assert.AreEqual(typeof(SubClass), baseClass.GetType());
            Assert.AreEqual(typeof(SubClass), parent2BaseClass.GetType());
            
            Assert.AreEqual(typeof(ConcreteAbstractClass), abstractClass.GetType());
            Assert.AreEqual(typeof(ConcreteAbstractClass), parent1AbstractClass.GetType());
            Assert.AreEqual(typeof(ConcreteAbstractClass), parent2AbstractClass.GetType());
            
            Assert.AreEqual(typeof(ConcreteInterface), childInterface.GetType());
            Assert.AreEqual(typeof(ConcreteInterface), parent1Interface.GetType());
            Assert.AreEqual(typeof(ConcreteInterface), parent2Interface.GetType());
            
            Assert.AreEqual(baseClass, parent2BaseClass);
            
            Assert.AreEqual(abstractClass, parent1AbstractClass);
            Assert.AreNotEqual(abstractClass, parent2AbstractClass);
            Assert.AreNotEqual(parent1AbstractClass, parent2AbstractClass);
            
            Assert.AreNotEqual(childInterface, parent1Interface);
            Assert.AreNotEqual(childInterface, parent2Interface);
            Assert.AreNotEqual(parent1Interface, parent2Interface);
        }
        
        [Test]
        public void CanChainInheritMultipleContainers()
        {
            var parent1Container = new ExtraContainer3(typeof(ExtraContainer1));//"Parent2");
            parent1Container.Bind<Interface, ConcreteInterface>().AsSingleton();
            parent1Container.Bind<AbstractClass, ConcreteAbstractClass>().AsSingleton();
            
            var parent2Container = new ExtraContainer1();
            parent2Container.Bind<Interface, ConcreteInterface>().AsSingleton();
            parent2Container.Bind<AbstractClass, ConcreteAbstractClass>().AsSingleton();
            parent2Container.Bind<BaseClass, SubClass>().AsSingleton();

            var container = new ExtraContainer2(typeof(ExtraContainer3));//"Parent1");
            container.Bind<Interface, ConcreteInterface>().AsSingleton();

            var baseClass = container.Resolve<BaseClass>();
            var parent2BaseClass = parent2Container.Resolve<BaseClass>();
            var abstractClass = container.Resolve<AbstractClass>();
            var parent1AbstractClass = parent1Container.Resolve<AbstractClass>();
            var parent2AbstractClass = parent2Container.Resolve<AbstractClass>();
            var parent1Interface = parent1Container.Resolve<Interface>();
            var parent2Interface = parent2Container.Resolve<Interface>();
            var childInterface = container.Resolve<Interface>();

            Assert.NotNull(baseClass);
            Assert.NotNull(parent2BaseClass);
            Assert.NotNull(abstractClass);
            Assert.NotNull(parent1AbstractClass);
            Assert.NotNull(parent2AbstractClass);
            Assert.NotNull(parent1Interface);
            Assert.NotNull(parent2Interface);
            Assert.NotNull(childInterface);
            
            Assert.AreEqual(typeof(SubClass), baseClass.GetType());
            Assert.AreEqual(typeof(SubClass), parent2BaseClass.GetType());
            
            Assert.AreEqual(typeof(ConcreteAbstractClass), abstractClass.GetType());
            Assert.AreEqual(typeof(ConcreteAbstractClass), parent1AbstractClass.GetType());
            Assert.AreEqual(typeof(ConcreteAbstractClass), parent2AbstractClass.GetType());
            
            Assert.AreEqual(typeof(ConcreteInterface), childInterface.GetType());
            Assert.AreEqual(typeof(ConcreteInterface), parent1Interface.GetType());
            Assert.AreEqual(typeof(ConcreteInterface), parent2Interface.GetType());
            
            Assert.AreEqual(baseClass, parent2BaseClass);
            
            Assert.AreEqual(abstractClass, parent1AbstractClass);
            Assert.AreNotEqual(abstractClass, parent2AbstractClass);
            Assert.AreNotEqual(parent1AbstractClass, parent2AbstractClass);
            
            Assert.AreNotEqual(childInterface, parent1Interface);
            Assert.AreNotEqual(childInterface, parent2Interface);
            Assert.AreNotEqual(parent1Interface, parent2Interface);
        }

        [Test]
        public void DisposedInheritedContainersNoLongerResolveBindings()
        {
            var parentContainer = new ExtraContainer1();
            parentContainer.Bind<Interface, ConcreteInterface>().AsSingleton();
            parentContainer.Bind<AbstractClass, ConcreteAbstractClass>();
        
            var container = new ExtraContainer2(typeof(ExtraContainer1));
            container.Bind<Interface, ConcreteInterface>().AsSingleton();

            var abstractClass = container.Resolve<AbstractClass>();
            var parentInterface = parentContainer.Resolve<Interface>();
            var childInterface = container.Resolve<Interface>();

            parentContainer.Dispose();

            Assert.Throws<Exception>(() => container.Resolve<AbstractClass>());
            Assert.Throws<Exception>(() => parentContainer.Resolve<Interface>());
            var childInterfaceSecond = container.Resolve<Interface>();
        
            Assert.NotNull(abstractClass);
            Assert.NotNull(parentInterface);
            Assert.NotNull(childInterface);
            Assert.NotNull(childInterfaceSecond);
            Assert.AreEqual(childInterface, childInterfaceSecond);
            Assert.AreNotEqual(childInterface, parentInterface);
        }
    }
}