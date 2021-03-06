﻿using Aspectus.CodeGen;
using Aspectus.Tests.BaseClasses;
using Microsoft.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Aspectus.Tests.CodeGen
{
    public class CompilerTests : TestingDirectoryFixture
    {
        [Fact]
        public void CreateMultipleTypes()
        {
            Logger.Information("CompilerTests.CreateMultipleTypes");
            using var Test = new Compiler("CreateMultipleTypes", ObjectPool);
            Test.Create("public class A{ public string Value1{get;set;}}", null, MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location));
            Test.Create("public class B{ public string Value1{get;set;}}", null, MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location));
            Test.Create("public class C{ public string Value1{get;set;}}", null, MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location));
            var TempAssembly = Test.Compile().LoadAssembly();
            var Object = TempAssembly.FirstOrDefault(x => x.FullName == "A");
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
            Test.Create("public class A{ public string Value1{get;set;}}", null, MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location));
            var TempAssembly = Test.Compile().LoadAssembly();
            var Object = TempAssembly.FirstOrDefault(x => x.FullName == "A");
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