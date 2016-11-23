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

using Aspectus.CodeGen.BaseClasses;
using System.Collections.Generic;
using System.Reflection;

namespace Aspectus.CodeGen
{
    /// <summary>
    /// Compiler
    /// </summary>
    public class Compiler : CompilerBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Compiler()
            : base("AspectusGeneratedTypes")
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        /// <param name="optimize">Optimize the assembly?</param>
        public Compiler(string assemblyName, bool optimize)
            : base(assemblyName, optimize)
        {
        }

        /// <summary>
        /// Compiles the specified code and returns the types that are created
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="usings">The usings.</param>
        /// <param name="references">The references.</param>
        /// <returns>The list of types that are generated</returns>
        public Compiler Create(string code, IEnumerable<string> usings, params Assembly[] references)
        {
            Add(code, usings, references);
            return this;
        }

        /// <summary>
        /// Creates a class object
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="code">Code</param>
        /// <param name="references">Assembly references</param>
        /// <param name="usings">Namespace usings</param>
        /// <returns>Type generated</returns>
        public Compiler CreateClass(string className, string code, IEnumerable<string> usings, params Assembly[] references)
        {
            Add(code, usings, references);
            return this;
        }
    }
}