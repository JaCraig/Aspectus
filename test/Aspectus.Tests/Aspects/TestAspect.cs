using Aspectus.Interfaces;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Aspectus.Tests.Aspects
{
    public interface IExample
    {
        string MySecretData { get; set; }
    }

    public class TestAspect : IAspect
    {
        public TestAspect()
        {
            AssembliesUsing = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(TestAspect).GetTypeInfo().Assembly.Location)
            };
            foreach (var DLL in new FileInfo(typeof(object).GetTypeInfo().Assembly.Location).Directory
                                                        .EnumerateFiles("*.dll")
                                                        .Where(x => Load.Contains(x.Name)))
            {
                var TempAssembly = MetadataReference.CreateFromFile(DLL.FullName);
                AssembliesUsing.Add(TempAssembly);
            }
        }

        private readonly string[] Load =
            {
            "mscorlib.dll",
"mscorlib.ni.dll",
"System.Collections.Concurrent.dll",
"System.Collections.dll",
"System.Collections.Immutable.dll",
"System.Runtime.dll"
        };

        public ICollection<MetadataReference> AssembliesUsing { get; }

        public ICollection<Type> InterfacesUsing => new Type[] { typeof(IExample) };

        public ICollection<string> Usings => new string[] { };

        public void Setup(object value)
        {
            if (value is IExample ExampleValue)
                ExampleValue.MySecretData = "BLAH";
        }

        public string SetupDefaultConstructor(Type baseType)
        {
            return "";
        }

        public string SetupEndMethod(MethodInfo method, Type baseType, string returnValueName)
        {
            return "";
        }

        public string SetupExceptionMethod(MethodInfo method, Type baseType)
        {
            return "";
        }

        public string SetupInterfaces(Type type)
        {
            return "public string MySecretData{get; set;}";
        }

        public string SetupStartMethod(MethodInfo method, Type baseType)
        {
            return "";
        }
    }
}