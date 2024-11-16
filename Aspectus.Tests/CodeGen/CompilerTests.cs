using Aspectus.CodeGen;
using Aspectus.Tests.BaseClasses;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace Aspectus.Tests.CodeGen
{
    public class CompilerTests : TestingDirectoryFixture
    {
        public CompilerTests()
        {
            _ObjectPool = ObjectPool;
            _AssemblyName = "TestValue941620842";
            _TestClass = new Compiler(_AssemblyName, _ObjectPool);
        }

        private readonly string _AssemblyName;

        private readonly ObjectPool<StringBuilder> _ObjectPool;

        private readonly Compiler _TestClass;

        [Fact]
        public void AssemblyNameIsInitializedCorrectly() => Assert.Equal(_AssemblyName, _TestClass.AssemblyName);

        [Fact]
        public void CanCallCompile()
        {
            // Arrange
            var TestClass = new Compiler("TestValue941620842", ObjectPool);
            _ = TestClass.Create("public class A{ public string Value1{get;set;}}", null, MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location));
            // Act
            global::Aspectus.CodeGen.BaseClasses.CompilerBase Result = TestClass.Compile();

            // Assert
            Assert.NotNull(Result);
        }

        [Fact]
        public void CanCallCreate()
        {
            // Arrange
            const string Code = "TestValue702238313";
            var Usings = new[] { "TestValue983550931", "TestValue1147447478", "TestValue723567240" };
            MetadataReference[] References = { MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(string)).Location), MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(string)).Location), MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(string)).Location) };

            // Act
            _ = _TestClass.Create(Code, Usings, References);
        }

        [Fact]
        public void CanCallLoadAssembly()
        {
            // Arrange
            var TestClass = new Compiler("TestValue", ObjectPool);
            // Act
            _ = Assert.Throws<BadImageFormatException>(TestClass.LoadAssembly);
        }

        [Fact]
        public void CanCallToString()
        {
            // Act
            var Result = _TestClass.ToString();

            // Assert
            Assert.NotNull(Result);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var Instance = new Compiler();

            // Assert
            Assert.NotNull(Instance);

            // Act
            Instance = new Compiler(_ObjectPool);

            // Assert
            Assert.NotNull(Instance);

            // Act
            Instance = new Compiler(_AssemblyName);

            // Assert
            Assert.NotNull(Instance);

            // Act
            Instance = new Compiler(_AssemblyName, _ObjectPool);

            // Assert
            Assert.NotNull(Instance);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void CanConstructWithInvalidAssemblyName(string value)
        {
            _ = new Compiler(value);
            _ = new Compiler(value, _ObjectPool);
        }

        [Fact]
        public void CanGetClasses()
        {
            // Assert
            List<Type> Result = Assert.IsType<List<Type>>(_TestClass.Classes);

            Assert.NotNull(Result);
            Assert.Empty(Result);
        }

        [Fact]
        public void CreateMultipleTypes()
        {
            Logger.Information("CompilerTests.CreateMultipleTypes");
            using var Test = new Compiler("CreateMultipleTypes", ObjectPool);
            _ = Test.Create("public class A{ public string Value1{get;set;}}", null, MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location));
            _ = Test.Create("public class B{ public string Value1{get;set;}}", null, MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location));
            _ = Test.Create("public class C{ public string Value1{get;set;}}", null, MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location));
            IEnumerable<Type> TempAssembly = Test.Compile().LoadAssembly();
            Type Object = TempAssembly.FirstOrDefault(x => x.FullName == "A");
            Assert.Equal("A", Object.FullName);
            Assert.Equal("CreateMultipleTypes.dll, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", Object.GetTypeInfo().Assembly.FullName);
            Object = TempAssembly.FirstOrDefault(x => x.FullName == "B");
            Assert.Equal("B", Object.FullName);
            Object = TempAssembly.FirstOrDefault(x => x.FullName == "C");
            Assert.Equal("C", Object.FullName);
        }

        [Fact]
        public void CreateType()
        {
            Logger.Information("CompilerTests.CreateType");
            using var Test = new Compiler("CreateType", ObjectPool);
            _ = Test.Create("public class A{ public string Value1{get;set;}}", null, MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location));
            IEnumerable<Type> TempAssembly = Test.Compile().LoadAssembly();
            Type Object = TempAssembly.FirstOrDefault(x => x.FullName == "A");
            Assert.Equal("CreateType.dll, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", Object.GetTypeInfo().Assembly.FullName);
            Assert.Equal("A", Object.FullName);
        }

        [Fact]
        public void Creation()
        {
            Logger.Information("CompilerTests.Creation");
            using var Test = new Compiler("Somewhere", ObjectPool);
            Assert.Empty(Test.Classes);
            Assert.Equal("Somewhere", Test.AssemblyName);
        }
    }
}