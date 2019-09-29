using System;
using System.Linq;
using NUnit.Framework;
using SF.IoC;

namespace SFIoCTest
{
    [TestFixture]
    public class ContextTests
    {
        private Container _container;
        
        [SetUp]
        public void Setup()
        {
            Context.Dispose();
            _container = new NameableTestContainer();
            _container.Bind<Interface, ConcreteInterface>();
            _container.Bind<AbstractClass, ConcreteAbstractClass>();
            _container.Bind<BaseClass, SubClassWithFieldDependencies>();
            _container.Bind<BaseClass, SubClassWithCircularDependencies>("Circle");
            _container.Bind<BaseClass, OtherSubClassWithCircularDependencies>("Other");
            _container.Bind<Interface, ConcreteInterface>("First");
            _container.Bind<AbstractClass, ConcreteAbstractClass>("Second");
            _container.Bind<BaseClass, SubClassWithCategory>("Third");
        }
        
        [Test]
        public void CanRetrieveContainerFromContextByName()
        {
            var container = Context.GetContainerByType<NameableTestContainer>();
            Assert.NotNull(container);
            Assert.AreEqual(typeof(NameableTestContainer), container.GetType());
            Assert.AreEqual(_container.GetBindings().Count, container.GetBindings().Count);
        }
        
        [Test]
        public void RetrieveContainerByNameFailsIfContainerNotBound()
        {
            Assert.Throws<Exception>(() =>
                {
                    Context.GetContainerByType(typeof(Interface));
                });
        }
        
        [Test]
        public void CanRetrieveContainerFromContextByTypeGeneric()
        {
            var container = Context.GetContainerByType<NameableTestContainer>();
            Assert.NotNull(container);
            Assert.AreEqual(typeof(NameableTestContainer), container.GetType());
            Assert.AreEqual(_container.GetBindings().Count, container.GetBindings().Count);
        }
        
        [Test]
        public void RetrieveContainerTypeGenericFailsIfContainerNotBoundByType()
        {
            Assert.Throws<Exception>(() =>
                {
                    Context.GetContainerByType<UnboundContainer>();
                });
        }
        
        [Test]
        public void RetrieveContainerTypeFailsIfContainerNotBoundByType()
        {
            Assert.Throws<Exception>(() =>
                {
                    Context.GetContainerByType(typeof(UnboundContainer));
                });
        }
        
        [Test]
        public void CanRetrieveContainerFromContextByType()
        {
            var container = Context.GetContainerByType<NameableTestContainer>();
            Assert.NotNull(container);
            Assert.AreEqual(typeof(NameableTestContainer), container.GetType());
            Assert.AreEqual(_container.GetBindings().Count, container.GetBindings().Count);
        }

        [Test]
        public void DisposeProperlyClearsContainers()
        {
            var container2 = new ExtraContainer1();
            container2.Bind<BaseClass, SubClassWithFieldDependencies>();
            container2.Bind<Interface, ConcreteInterface>("First").AsSingleton();
            container2.Bind<Interface, ConcreteInterface>();
            var abstractClass = _container.Resolve<AbstractClass>();
            var baseClass = container2.Resolve<BaseClass>();
            var bindings = container2.GetBindings();
            var binding = bindings.FirstOrDefault(t => t.Item2.TypeBoundFrom == typeof(BaseClass))?.Item2;
            var bindingInterface = bindings.FirstOrDefault(t => t.Item1 == "First")?.Item2;
            var dependencies = binding?.GetDependencies();
            
            Assert.NotNull(baseClass);
            Assert.NotNull(abstractClass);
            Assert.NotNull(bindings);
            Assert.NotNull(binding);
            Assert.NotNull(bindingInterface);
            Assert.NotNull(dependencies);
            Assert.AreEqual(3, bindings.Count);
            Assert.AreEqual(2, dependencies.Count);
            Assert.IsTrue(bindingInterface.HasInstanceAvailable());
            Context.Dispose();

            Assert.Throws<Exception>(() => Context.GetContainerByType<ExtraContainer1>());
            Assert.Throws<Exception>(() => Context.GetContainerByType(typeof(ExtraContainer1)));
            Assert.Throws<Exception>(() => _container.Resolve<AbstractClass>());
            Assert.IsFalse(bindingInterface.HasInstanceAvailable());
            Assert.AreEqual(0, _container.GetBindings().Count);

        }
        // Dispose clears all containers and bindings and save instances
    }
}