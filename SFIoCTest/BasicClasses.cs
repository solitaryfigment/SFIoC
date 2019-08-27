using SF.IoC;

namespace SFIoCTest
{
    public class SubClassWithConstructorArgs : BaseClass
    {
        public float F;
        public int I;
        public SubClassWithConstructorArgs(int i, float f)
        {
            I = i;
            F = f;
        }
    }
    public class SubClassWithCircularDependencies : BaseClass
    {
        [Inject] public Interface Interface;
        [Inject("Other")] public BaseClass Circular;
    }

    public class OtherSubClassWithCircularDependencies : BaseClass
    {
        [Inject] public Interface Interface;
        [Inject("Circle")] public BaseClass Circle;
    }
    
    public class SubClassWithFieldDependencies : BaseClass
    {
        [Inject] public Interface Interface;
        [Inject("First")] public Interface InterfaceWithCategory;
    }
    
    public class SubClassWithPropetyDependencies : BaseClass
    {
        [Inject]
        public Interface Interface { get; set; }

        [Inject("First")]
        public Interface InterfaceWithCategory { get; set; }
    }
    
    public class SubClassWithConstructorDependencies : BaseClass
    {
        public Interface Interface { get; set; }

        public AbstractClass AbstractClass { get; set; }

        [DefaultConstructor]
        public SubClassWithConstructorDependencies(Interface @interface, [InjectArgument("First")] AbstractClass abstractClass)
        {
            Interface = @interface;
            AbstractClass = abstractClass;
        }
    }
    
    public class DefaultConstructorCircularDependency : BaseClass
    {
        public BaseClass BaseClass { get; set; }

        [DefaultConstructor]
        public DefaultConstructorCircularDependency([InjectArgument("Second")] BaseClass baseClass)
        {
            BaseClass = baseClass;
        }
    }
    
    public class DefaultConstructorCircularDependencyOther : BaseClass
    {
        public BaseClass BaseClass { get; set; }


        [DefaultConstructor]
        public DefaultConstructorCircularDependencyOther([InjectArgument("First")] BaseClass baseClass)
        {
            BaseClass = baseClass;
        }
    }

    public class SubSubClassWithConstructorDependencies : SubClassWithConstructorDependencies
    {
        public SubSubClassWithConstructorDependencies(Interface @interface, [InjectArgument("First")] AbstractClass abstractClass) : base(@interface, abstractClass)
        {
        }
    }
    
    public abstract class AbstractClass
    {
        
    }

    public class ConcreteAbstractClass : AbstractClass
    {
        
    }
    
    public class BaseClass
    {
        
    }

    public class SubClass : BaseClass
    {
        
    }

    public class SubClassWithCategory : BaseClass
    {
        
    }

    public class ConcreteInterface : Interface
    {
        
    }

    public interface Interface
    {
        
    }

    public class TestContainer : Container
    {
        public TestContainer() : base(nameof(TestContainer))
        {
        }
    }
    
    public class NameableTestContainer : Container
    {
        public NameableTestContainer(string name) : base(name)
        {
        }
    }
    
    public class InheritTestContainer : Container
    {
        public InheritTestContainer(string name, params string[] inheritedContainer) : base(name, inheritedContainer)
        {
        }
    }
}
