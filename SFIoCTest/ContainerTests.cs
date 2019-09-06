using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
            _container = new NameableTestContainer("ContainerTests");
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
        public void CanMakeValidBindings()
        {
            Context.Dispose();
            var container = new TestContainer();
            container.Bind<Interface, ConcreteInterface>();
            container.Bind<AbstractClass, ConcreteAbstractClass>();
            container.Bind<BaseClass, SubClass>();
            container.Bind<Interface, ConcreteInterface>("First");
            container.Bind<AbstractClass, ConcreteAbstractClass>("Second");
            container.Bind<BaseClass, SubClass>("Third");

            int index = 0;
            var bindings = container.GetBindings();
            Assert.AreEqual(6, bindings.Count);
            Assert.AreEqual(string.Empty, bindings[index].Item1);
            Assert.AreSame(typeof(Interface), bindings[index].Item2.TypeBoundFrom);
            Assert.AreSame(typeof(ConcreteInterface), bindings[index].Item2.TypeBoundTo);
            Assert.AreEqual("First", bindings[++index].Item1);
            Assert.AreSame(typeof(Interface), bindings[index].Item2.TypeBoundFrom);
            Assert.AreSame(typeof(ConcreteInterface), bindings[index].Item2.TypeBoundTo);
            Assert.AreEqual(string.Empty, bindings[++index].Item1);
            Assert.AreSame(typeof(AbstractClass), bindings[index].Item2.TypeBoundFrom);
            Assert.AreSame(typeof(ConcreteAbstractClass), bindings[index].Item2.TypeBoundTo);
            Assert.AreEqual("Second", bindings[++index].Item1);
            Assert.AreSame(typeof(AbstractClass), bindings[index].Item2.TypeBoundFrom);
            Assert.AreSame(typeof(ConcreteAbstractClass), bindings[index].Item2.TypeBoundTo);
            Assert.AreEqual(string.Empty, bindings[++index].Item1);
            Assert.AreSame(typeof(BaseClass), bindings[index].Item2.TypeBoundFrom);
            Assert.AreSame(typeof(SubClass), bindings[index].Item2.TypeBoundTo);
            Assert.AreEqual("Third", bindings[++index].Item1);
            Assert.AreSame(typeof(BaseClass), bindings[index].Item2.TypeBoundFrom);
            Assert.AreSame(typeof(SubClass), bindings[index].Item2.TypeBoundTo);
        }
        
        [Test]
        public void CanResolveType()
        {
            var obj = _container.Resolve<Interface>();
            Assert.NotNull(obj);
            Assert.AreEqual(typeof(ConcreteInterface),obj.GetType());
        }
        
        [Test]
        public void CanResolveTypeWithCategory()
        {
            var obj = _container.Resolve<BaseClass>("Third");
            Assert.NotNull(obj);
            Assert.AreEqual(typeof(SubClassWithCategory),obj.GetType());
        }

        [Test]
        public void FindsDependencies()
        {
            var methodInfo = _container.GetType().GetMethod("FindBinding", BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic);
            Assert.NotNull(methodInfo);
            var binding = methodInfo.Invoke(_container, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, null, new object[]{typeof(BaseClass), ""}, CultureInfo.CurrentCulture) as IBinding;
            Assert.NotNull(binding);
            Assert.AreEqual(typeof(SubClassWithFieldDependencies), binding.TypeBoundTo);
            var dependencies = binding.GetDependencies();
            
            Assert.NotNull(dependencies);
            Assert.IsTrue(dependencies.Count > 0);
            Assert.AreEqual(2, dependencies.Count(d => d.Type == typeof(Interface)));

            var noCategoryDependency = dependencies.FirstOrDefault(d => d.Category == "" && d.Type == typeof(Interface));
            Assert.NotNull(noCategoryDependency);
            Assert.AreEqual("Interface", noCategoryDependency.MemberName);
            Assert.AreEqual(MemberTypes.Field, noCategoryDependency.MemberType);
            
            var categoryDependency = dependencies.FirstOrDefault(d => d.Category == "First" && d.Type == typeof(Interface));
            Assert.NotNull(categoryDependency);
            Assert.AreEqual("InterfaceWithCategory", categoryDependency.MemberName);
            Assert.AreEqual(MemberTypes.Field, categoryDependency.MemberType);
        }
        
        [Test]
        public void ResolveTypeWithDependencies()
        {
            var instance = _container.Resolve<BaseClass>() as SubClassWithFieldDependencies;            
            Assert.NotNull(instance);

            var noCategoryDependency = instance.Interface;
            Assert.NotNull(noCategoryDependency);
            Assert.AreEqual(typeof(ConcreteInterface), noCategoryDependency.GetType());

            var categoryDependency = instance.InterfaceWithCategory;
            Assert.NotNull(categoryDependency);
            Assert.AreEqual(typeof(ConcreteInterface), categoryDependency.GetType());
        }
        
        [Test]
        public void ResolveTypeWithCircleDependenciesFail()
        {
            Assert.Throws<CircularDependencyException>(() => _container.Resolve<BaseClass>("Circle"));
        }
        
        [Test]
        public void CanResolveTypeWithSingletonCircleDependencies()
        {
            var container = new NameableTestContainer("OtherContainer");
            container.Bind<Interface, ConcreteInterface>();
            container.Bind<AbstractClass, ConcreteAbstractClass>();
            container.Bind<BaseClass, SubClassWithFieldDependencies>();
            container.Bind<BaseClass, SubClassWithCircularDependencies>("Circle").AsSingleton();
            container.Bind<BaseClass, OtherSubClassWithCircularDependencies>("Other").AsSingleton();
            container.Bind<Interface, ConcreteInterface>("First");
            container.Bind<AbstractClass, ConcreteAbstractClass>("Second");
            container.Bind<BaseClass, SubClassWithCategory>("Third");
            
            var instance = container.Resolve<BaseClass>("Circle") as SubClassWithCircularDependencies;            
            Assert.NotNull(instance);

            var dependencyInterface = instance.Interface;
            Assert.NotNull(dependencyInterface);
            Assert.AreEqual(typeof(ConcreteInterface), dependencyInterface.GetType());

            var circular = instance.Circular;
            Assert.NotNull(circular);
            Assert.AreEqual(typeof(OtherSubClassWithCircularDependencies), circular.GetType());
            
            var otherInterface = ((OtherSubClassWithCircularDependencies)circular).Interface;
            Assert.NotNull(otherInterface);
            Assert.AreEqual(typeof(ConcreteInterface), otherInterface.GetType());

            var otherCircular = ((OtherSubClassWithCircularDependencies)circular).Circle;
            Assert.NotNull(otherCircular);
            Assert.AreEqual(typeof(SubClassWithCircularDependencies), otherCircular.GetType());
            Assert.AreEqual(instance, otherCircular);
        }

        [Test]
        public void ContainerNameIsSetProperly()
        {
            var container = new NameableTestContainer("NameOfContainer");
            Assert.AreEqual("NameOfContainer", container.Name);
        }

        [Test]
        public void TwoContainerCannotHaveTheSameName()
        {
            var container = new NameableTestContainer("TwoContainerCannotHaveTheSameName");
            Assert.Throws<Exception>(() => new NameableTestContainer("TwoContainerCannotHaveTheSameName"));
        }

        [Test]
        public void TwoContainerOfTheSameTypeCanHaveDifferentNames()
        {
            var container = new NameableTestContainer("ContainerName1");
            var container2 = new NameableTestContainer("TwoContainerCannotHaveTheSameName");
            Assert.Pass();
        }

        [Test]
        public void DisposingAContainerRemovesFromTheContext()
        {
            var container = new NameableTestContainer("DisposedContainerName");
            container.Dispose();
            Assert.Throws<Exception>(() => Context.GetContainerByName("DisposedContainerName"));
            Assert.Throws<Exception>(() => Context.GetContainerByType<NameableTestContainer>("DisposedContainerName"));
            Assert.Throws<Exception>(() => Context.GetContainerByType(typeof(NameableTestContainer), "DisposedContainerName"));
        }

        [Test]
        public void BindingsOfSameTypeButDifferentCategoriesReturnDifferentObjectsForTransient()
        {
            var container = new NameableTestContainer("DifferentBindingContainerName");
            container.Bind<Interface, ConcreteInterface>("Binding1");
            container.Bind<Interface, ConcreteInterface>("Binding2");

            var interface1 = container.Resolve<Interface>("Binding1");
            var interface2 = container.Resolve<Interface>("Binding2");
            
            Assert.NotNull(interface1);
            Assert.NotNull(interface2);
            Assert.AreNotEqual(interface1, interface2);
        }

        [Test]
        public void BindingsOfSameTypeButDifferentCategoriesReturnDifferentObjectsForSingleton()
        {
            var container = new NameableTestContainer("DifferentBindingContainerName2");
            container.Bind<Interface, ConcreteInterface>("Binding1").AsSingleton();
            container.Bind<Interface, ConcreteInterface>("Binding2").AsSingleton();

            var interface1 = container.Resolve<Interface>("Binding1");
            var interface2 = container.Resolve<Interface>("Binding2");
            
            Assert.NotNull(interface1);
            Assert.NotNull(interface2);
            Assert.AreNotEqual(interface1, interface2);
        }

        [Test]
        public void BindingsOfSameTypeButDifferentCategoriesReturnDifferentObjectsForInstanced()
        {
            var container = new NameableTestContainer("DifferentBindingContainerName2");
            container.Bind<Interface, ConcreteInterface>("Binding1", new ConcreteInterface());
            container.Bind<Interface, ConcreteInterface>("Binding2", new ConcreteInterface());

            var interface1 = container.Resolve<Interface>("Binding1");
            var interface2 = container.Resolve<Interface>("Binding2");
            
            Assert.NotNull(interface1);
            Assert.NotNull(interface2);
            Assert.AreNotEqual(interface1, interface2);
        }

        [Test]
        public void BindingsGivenTheSameInstanceResolveToTheSameObject()
        {
            var instance = new ConcreteInterface();
            var container = new NameableTestContainer("DifferentBindingContainerName2");
            container.Bind<Interface, ConcreteInterface>("Binding1", instance);
            container.Bind<Interface, ConcreteInterface>("Binding2", instance);

            var interface1 = container.Resolve<Interface>("Binding1");
            var interface2 = container.Resolve<Interface>("Binding2");
            
            Assert.NotNull(interface1);
            Assert.NotNull(interface2);
            Assert.AreEqual(interface1, interface2);
        }

        [Test]
        public void CanResolveTypeWithDefaultConstructor()
        {
            var container = new NameableTestContainer("DefaultConstructor");
            container.Bind<BaseClass, SubClassWithConstructorDependencies>();
            container.Bind<Interface, ConcreteInterface>();
            container.Bind<AbstractClass, ConcreteAbstractClass>("First");

            var baseClass = container.Resolve<BaseClass>() as SubClassWithConstructorDependencies;
            
            Assert.NotNull(baseClass);
            Assert.NotNull(baseClass.Interface);
            Assert.NotNull(baseClass.AbstractClass);
            Assert.AreEqual(typeof(ConcreteInterface), baseClass.Interface.GetType());
            Assert.AreEqual(typeof(ConcreteAbstractClass), baseClass.AbstractClass.GetType());
        }

        [Test]
        public void CanResolveTypeWithDefaultConstructorInInheritedClass()
        {
            var container = new NameableTestContainer("InheritedDefaultConstructor");
            container.Bind<BaseClass, SubSubClassWithConstructorDependencies>();
            container.Bind<Interface, ConcreteInterface>();
            container.Bind<AbstractClass, ConcreteAbstractClass>("First");

            var baseClass = container.Resolve<BaseClass>() as SubClassWithConstructorDependencies;
            
            Assert.NotNull(baseClass);
            Assert.NotNull(baseClass.Interface);
            Assert.NotNull(baseClass.AbstractClass);
            Assert.AreEqual(typeof(ConcreteInterface), baseClass.Interface.GetType());
            Assert.AreEqual(typeof(ConcreteAbstractClass), baseClass.AbstractClass.GetType());
        }

        [Test]
        public void TransientBindingsTypeWithDefaultConstructorFailIfCircularDependencies()
        {
            var container = new NameableTestContainer("CircularDefaultConstructor1");
            container.Bind<BaseClass, DefaultConstructorCircularDependency>("First");
            container.Bind<BaseClass, DefaultConstructorCircularDependencyOther>("Second");

            ThrowsInnerException<CircularDependencyException>(() => container.Resolve<BaseClass>("First"));
        }

        [Test]
        public void SingletonBindingsTypeWithDefaultConstructorFailIfCircularDependencies()
        {
            var container = new NameableTestContainer("CircularDefaultConstructor2");
            container.Bind<BaseClass, DefaultConstructorCircularDependency>("First").AsSingleton();
            container.Bind<BaseClass, DefaultConstructorCircularDependencyOther>("Second").AsSingleton();

            ThrowsInnerException<CircularDependencyException>(() => container.Resolve<BaseClass>("First"));
        }

        [Test]
        public void OneInstancedBindingsTypeWithDefaultConstructorSucceedsIfCircularDependencies()
        {
            var first = new DefaultConstructorCircularDependency(null);
            var second = new DefaultConstructorCircularDependencyOther(first);
            var container = new NameableTestContainer("CircularDefaultConstructor3");
            container.Bind<BaseClass, DefaultConstructorCircularDependency>("First");
            container.Bind<BaseClass, DefaultConstructorCircularDependencyOther>("Second", second);

            var firstResolved = container.Resolve<BaseClass>("First") as DefaultConstructorCircularDependency;
            Assert.NotNull(firstResolved);
            var secondResolved = firstResolved.BaseClass as DefaultConstructorCircularDependencyOther;
            Assert.NotNull(secondResolved);
            var firstCircularResolved = secondResolved.BaseClass as DefaultConstructorCircularDependency;
            Assert.NotNull(firstCircularResolved);
            var secondShouldBePassedIn = firstCircularResolved.BaseClass;
            Assert.IsNull(secondShouldBePassedIn);
            
            Assert.AreNotEqual(firstResolved, firstCircularResolved);
            Assert.AreEqual(second, secondResolved);
        }

        public void ThrowsInnerException<T>(Action code) where T : Exception
        {
            try
            {
                if(code == null)
                {
                    Assert.Fail("Code to test cannot be null.");
                }
                code();
            }
            catch(Exception exception)
            {
                Exception innerException = exception; 
                while(!(innerException is T))
                {
                    if(innerException.InnerException != null)
                    {
                        innerException = innerException.InnerException;
                    }
                    else
                    {
                        break;
                    }
                }
                Assert.AreEqual(typeof(T), innerException.GetType());
            }
        }
    }
}
