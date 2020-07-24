using System;
using SF.IoC;

namespace SFIoCTest
{
    

    public class ExtraContainer1 : Container
    {
        public ExtraContainer1() : base()
        {
        }

        public ExtraContainer1(params Type[] inheritedContainers) : base(inheritedContainers)
        {
        }

        protected override void SetBindings()
        {
        }
    }

    public class ExtraContainer2 : Container
    {
        public ExtraContainer2() : base()
        {
        }

        public ExtraContainer2(params Type[] inheritedContainers) : base(inheritedContainers)
        {
        }

        protected override void SetBindings()
        {
        }
    }

    public class ExtraContainer3 : Container
    {
        public ExtraContainer3() : base()
        {
        }

        public ExtraContainer3(params Type[] inheritedContainers) : base(inheritedContainers)
        {
        }

        protected override void SetBindings()
        {
        }
    }
    
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
    
    public class SubSubClass : SubClass
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
        public TestContainer() : base()
        {
        }

        protected override void SetBindings()
        {
            
        }
    }
    
    public class NameableTestContainer : Container
    {
        public NameableTestContainer() : base()
        {
        }

        protected override void SetBindings()
        {
            
        }
    }
    
    public class InheritTestContainer : Container
    {
        public InheritTestContainer(params Type[] inheritedContainer) : base(inheritedContainer)
        {
        }

        protected override void SetBindings()
        {
            
        }
    }

    public class UnboundContainer : Container
    {
        public UnboundContainer() : base()
        {
        }

        protected override void SetBindings()
        {
            
        }
    }
}
