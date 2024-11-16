using Aspectus.CodeGen;
using Aspectus.Interfaces;
using Aspectus.Tests.Aspects;
using Aspectus.Tests.BaseClasses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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
        public AspectusTests()
        {
            _compiler = new Compiler();
            _aspects = new[] { Substitute.For<IAspect>(), Substitute.For<IAspect>(), Substitute.For<IAspect>() };
            _modules = new[] { Substitute.For<IAOPModule>(), Substitute.For<IAOPModule>(), Substitute.For<IAOPModule>() };
            _logger = Substitute.For<ILogger<Aspectus>>();
            _objectPool = new DefaultObjectPoolProvider().CreateStringBuilderPool(10, 10);
            _testClass = new Aspectus(_compiler, _aspects, _modules, _logger, _objectPool);
        }

        private readonly IEnumerable<IAspect> _aspects;

        private readonly Compiler _compiler;
        private readonly ILogger<Aspectus> _logger;

        private readonly IEnumerable<IAOPModule> _modules;

        private readonly ObjectPool<StringBuilder> _objectPool;

        private readonly Aspectus _testClass;

        [Fact]
        public void CanCallCreateWithBaseType()
        {
            // Arrange
            Type baseType = typeof(AOPTestClass);

            // Act
            var Result = _testClass.Create(baseType);

            // Assert
            Assert.NotNull(Result);
        }

        [Fact]
        public void CanCallCreateWithT()
        {
            // Act
            AOPTestClass Result = _testClass.Create<AOPTestClass>();

            // Assert
            Assert.NotNull(Result);
        }

        [Fact]
        public void CanCallFinalizeSetup() =>
            // Act
            _testClass.FinalizeSetup();

        [Fact]
        public void CanCallSetupWithAssemblies()
        {
            // Arrange
            var TestClass = new Aspectus(new Compiler(), new List<IAspect>(), new List<IAOPModule>(), Substitute.For<ILogger<Aspectus>>(), new DefaultObjectPoolProvider().CreateStringBuilderPool(10, 10));
            Assembly[] assemblies = new[] { Assembly.GetAssembly(typeof(AspectusTests)) };

            // Act
            TestClass.Setup(assemblies);
        }

        [Fact]
        public void CanCallSetupWithTypes()
        {
            // Arrange
            Type[] types = new[] { typeof(AOPTestClass), typeof(AOPTestClass2) };

            // Act
            _testClass.Setup(types);
        }

        [Fact]
        public void CanCallToString()
        {
            // Act
            var Result = _testClass.ToString();

            // Assert
            Assert.NotNull(Result);
            Assert.NotEmpty(Result);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new Aspectus(_compiler, _aspects, _modules, _logger, _objectPool);

            // Assert
            Assert.NotNull(instance);

            // Act
            instance = new Aspectus(_compiler, _aspects, _modules);

            // Assert
            Assert.NotNull(instance);
        }

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
            Aspectus Test = GetServiceProvider().GetService<Aspectus>();
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