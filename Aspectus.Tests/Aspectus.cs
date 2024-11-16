using Aspectus.CodeGen;
using Aspectus.Interfaces;
using Aspectus.Tests.Aspects;
using Aspectus.Tests.BaseClasses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
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
            Logger.Information("AspectusTests.Create");
            var Test = new Aspectus(new Compiler(ObjectPool), new List<IAspect>(), new List<IAOPModule>(), GetServiceProvider().GetService<ILogger<Aspectus>>(), ObjectPool);
            var Item = (AOPTestClass)Test.Create(typeof(AOPTestClass));
            Assert.NotNull(Item);
        }

        [Fact]
        public void TestAspectFromCanister()
        {
            Logger.Information("AspectusTests.TestAspectTestMultiple");
            var Test = GetServiceProvider().GetService<Aspectus>();
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

        [Fact]
        public void TestAspectTest()
        {
            Logger.Information("AspectusTests.TestAspectTest");
            var Test = new Aspectus(new Compiler("TestAspectTest", ObjectPool), new[] { new TestAspect() }.ToList(), new List<IAOPModule>(), GetServiceProvider().GetService<ILogger<Aspectus>>(), ObjectPool);
            Test.Setup(typeof(AOPTestClass), typeof(AOPTestClass2));
            var Item = (AOPTestClass)Test.Create(typeof(AOPTestClass));
            Assert.NotNull(Item);
            var ExampleItem = Item as IExample;
            Assert.NotNull(ExampleItem);
            Assert.Equal("BLAH", ExampleItem.MySecretData);
        }

        [Fact]
        public void TestAspectTestAfterClean()
        {
            Logger.Information("AspectusTests.TestAspectTestAfterClean");
            var Test = new Aspectus(new Compiler("TestAspectTestAfterClean", ObjectPool), new[] { new TestAspect() }.ToList(), new List<IAOPModule>(), GetServiceProvider().GetService<ILogger<Aspectus>>(), ObjectPool);
            Test.FinalizeSetup();
            Test.Setup(typeof(AOPTestClass), typeof(AOPTestClass2));
            var Item = (AOPTestClass)Test.Create(typeof(AOPTestClass));
            Assert.NotNull(Item);
            var ExampleItem = Item as IExample;
            Assert.Null(ExampleItem);
            var Item2 = (AOPTestClass2)Test.Create(typeof(AOPTestClass2));
            Assert.NotNull(Item2);
            ExampleItem = Item2 as IExample;
            Assert.Null(ExampleItem);
        }

        [Fact]
        public void TestAspectTestBeforeClean()
        {
            Logger.Information("AspectusTests.TestAspectTestBeforeClean");
            var Test = new Aspectus(new Compiler("TestAspectTestBeforeClean", ObjectPool), new[] { new TestAspect() }.ToList(), new List<IAOPModule>(), GetServiceProvider().GetService<ILogger<Aspectus>>(), ObjectPool);
            Test.Setup(typeof(AOPTestClass), typeof(AOPTestClass2));
            Test.FinalizeSetup();
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

        [Fact]
        public void TestAspectTestMultiple()
        {
            Logger.Information("AspectusTests.TestAspectTestMultiple");
            var Test = new Aspectus(new Compiler("TestAspectTestMultiple", ObjectPool), new[] { new TestAspect() }.ToList(), new List<IAOPModule>(), GetServiceProvider().GetService<ILogger<Aspectus>>(), ObjectPool);
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