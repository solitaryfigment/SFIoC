using NUnit.Framework;
using SF.IoC;

namespace SFIoCTest
{
    [TestFixture]
    public class ContainerTests
    {
        private Container _container;

        [SetUp]
        public void Setup()
        {
            Context.Dispose();
            _container = new NameableTestContainer();
        }

        [Test]
        public void CanGetConstructorDependenciesNonDefaultConstructor()
        {
            _container.Bind<BaseClass, SubClassWithConstructorDependencies2>();
            _container.Bind<Interface, ConcreteInterface>();
            _container.Bind<AbstractClass, ConcreteAbstractClass>();
            
            var obj = _container.Resolve<BaseClass>();
            
            Assert.NotNull(obj);
        }
    }
}

