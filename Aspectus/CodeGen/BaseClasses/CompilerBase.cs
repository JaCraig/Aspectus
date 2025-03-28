﻿/*
Copyright 2016 James Craig

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using Aspectus.HelperFunctions;
using Fast.Activator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace Aspectus.CodeGen.BaseClasses
{
    /// <summary>
    /// Compiler base class
    /// </summary>
    public abstract class CompilerBase : IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assemblyName">Assembly name to save the generated types as</param>
        /// <param name="objectPool">The object pool.</param>
        protected CompilerBase(string assemblyName, ObjectPool<StringBuilder>? objectPool = null)
        {
            AssemblyName = assemblyName;
            var DebuggableAttribute = Assembly.GetEntryAssembly()?.GetCustomAttribute<DebuggableAttribute>();
            Optimize = CheckJitProperty(DebuggableAttribute);
            Code = objectPool?.Get() ?? new StringBuilder();
            ObjectPool = objectPool;
        }

        /// <summary>
        /// Assembly name
        /// </summary>
        public string AssemblyName { get; }

        /// <summary>
        /// Dictionary containing generated types and associates it with original type
        /// </summary>
        public List<Type> Classes { get; private set; } = [];

        /// <summary>
        /// Gets the assembly stream.
        /// </summary>
        /// <value>The assembly stream.</value>
        protected MemoryStream? AssemblyStream { get; private set; } = new MemoryStream();

        /// <summary>
        /// Should this be optimized?
        /// </summary>
        protected bool Optimize { get; }

        /// <summary>
        /// Gets or sets the assemblies.
        /// </summary>
        /// <value>The assemblies.</value>
        private List<MetadataReference> Assemblies { get; } = [];

        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>The code.</value>
        private StringBuilder Code { get; }

        /// <summary>
        /// Gets the object pool.
        /// </summary>
        /// <value>The object pool.</value>
        private ObjectPool<StringBuilder>? ObjectPool { get; set; }

        /// <summary>
        /// Gets or sets the usings.
        /// </summary>
        /// <value>The usings.</value>
        private List<string> Usings { get; } = [];

        /// <summary>
        /// Compiles this instance.
        /// </summary>
        /// <returns>This</returns>
        /// <exception cref="System.Exception">Any errors that are sent back by Roslyn</exception>
        public CompilerBase Compile()
        {
            var CSharpCompiler = CSharpCompilation.Create(AssemblyName + ".dll")
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, usings: Usings, optimizationLevel: Optimize ? OptimizationLevel.Release : OptimizationLevel.Debug))
                .AddReferences(Assemblies)
                .AddSyntaxTrees([CSharpSyntaxTree.ParseText(Code.ToString())]);
            using (var TempStream = new MemoryStream())
            {
                var Result = CSharpCompiler.Emit(TempStream);
                if (!Result.Success)
                {
                    var ErrorText = Code + Environment.NewLine + Environment.NewLine + Result.Diagnostics.ToString(x => x.GetMessage() + " : " + x.Location.GetLineSpan().StartLinePosition.Line, Environment.NewLine);
                    throw new Exception(ErrorText);
                }
                var MiniAssembly = TempStream.ToArray();
                AssemblyStream?.Write(MiniAssembly, 0, MiniAssembly.Length);
            }
            return this;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Loads the assembly into memory.
        /// </summary>
        /// <returns>The types within the assembly</returns>
        public IEnumerable<Type> LoadAssembly()
        {
            if (AssemblyStream is null)
                return Classes;
            AssemblyStream.Seek(0, SeekOrigin.Begin);
            var ResultingAssembly = AssemblyLoadContext.Default.LoadFromStream(AssemblyStream, null);
            Classes.AddIfUnique((x, y) => x.FullName == y.FullName, ResultingAssembly.GetTypes());
            return Classes;
        }

        /// <summary>
        /// Outputs basic information about the compiler as a string
        /// </summary>
        /// <returns>The string version of the compiler</returns>
        public override string ToString() => "Compiler: " + AssemblyName + "\r\n";

        /// <summary>
        /// Creates an object using the type specified
        /// </summary>
        /// <typeparam name="T">Type to cast to</typeparam>
        /// <param name="typeToCreate">Type to create</param>
        /// <param name="args">Args to pass to the constructor</param>
        /// <returns>The created object</returns>
        /// <exception cref="ArgumentNullException">typeToCreate</exception>
        protected static T Create<T>(Type typeToCreate, params object[] args)
        {
            ArgumentNullException.ThrowIfNull(typeToCreate, nameof(typeToCreate));
            return (T)FastActivator.CreateInstance(typeToCreate, args);
        }

        /// <summary>
        /// Adds the specified code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="usings">The usings.</param>
        /// <param name="references">The references.</param>
        /// <returns>This</returns>
        protected CompilerBase Add(string code, IEnumerable<string> usings, params MetadataReference[] references)
        {
            if (AssemblyStream is null)
                return this;
            Code.AppendLine(code);
            Usings.AddIfUnique(usings);
            Assemblies.AddIfUnique(references);
            return this;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="managed">
        /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release
        /// only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool managed)
        {
            if (managed)
            {
                ObjectPool?.Return(Code);
                ObjectPool = null;
                AssemblyStream?.Dispose();
                AssemblyStream = null;
                Classes = [];
            }
        }

        /// <summary>
        /// Checks the JIT property.
        /// </summary>
        /// <param name="debuggableAttribute">The debuggable attribute.</param>
        /// <returns>True if in Release mode, false otherwise</returns>
        private static bool CheckJitProperty(DebuggableAttribute? debuggableAttribute)
        {
            return debuggableAttribute != null
                && !(bool)(debuggableAttribute.GetType().GetProperty("IsJITOptimizerDisabled")?.GetValue(debuggableAttribute) ?? true);
        }
    }
}