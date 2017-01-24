using Aspectus.CodeGen;
using Aspectus.Interfaces;
using Aspectus.Tests.Aspects;
using Aspectus.Tests.BaseClasses;
using System.Collections.Generic;
using Xunit;

namespace Aspectus.Tests
{
    public class AOPTestClass
    {
        public virtual string A { get; set; }

        public virtual int B { get; set; }

        public virtual float C { get; set; }

        public virtual List<string> D { get; set; }
    }

    public class AOPTestClass2
    {
        public virtual string A { get; set; }

        public virtual int B { get; set; }

        public virtual float C { get; set; }

        public virtual List<string> D { get; set; }
    }

    public class AspectusTests : TestingDirectoryFixture
    {
        [Fact]
        public void Create()
        {
            var Test = new Aspectus(new Compiler(), new List<IAspect>(), new List<IAOPModule>());
            var Item = (AOPTestClass)Test.Create(typeof(AOPTestClass));
            Assert.NotNull(Item);
        }

        [Fact]
        public void TestAspectTest()
        {
            var Test = Canister.Builder.Bootstrapper.Resolve<Aspectus>();
            Test.Setup(typeof(AOPTestClass), typeof(AOPTestClass2));
            var Item = (AOPTestClass)Test.Create(typeof(AOPTestClass));
            Assert.NotNull(Item);
            var ExampleItem = Item as IExample;
            Assert.NotNull(ExampleItem);
            Assert.Equal("BLAH", ExampleItem.MySecretData);
        }

        [Fact]
        public void TestAspectTestMultiple()
        {
            var Test = Canister.Builder.Bootstrapper.Resolve<Aspectus>();
            Test.Setup(typeof(AOPTestClass), typeof(AOPTestClass2));
            var Item = (AOPTestClass)Test.Create(typeof(AOPTestClass));
            Assert.NotNull(Item);
            var ExampleItem = Item as IExample;
            Assert.NotNull(ExampleItem);
            Assert.Equal("BLAH", ExampleItem.MySecretData);
            var Item2 = (AOPTestClass2)Test.Create(typeof(AOPTestClass2));
            Assert.NotNull(Item2);
            ExampleItem = Item2 as IExample;
            Assert.NotNull(ExampleItem);
            Assert.Equal("BLAH", ExampleItem.MySecretData);
        }
    }
}