/*
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

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Aspectus.Interfaces
{
    /// <summary>
    /// Aspect interface
    /// </summary>
    public interface IAspect
    {
        /// <summary>
        /// Set of assemblies that the aspect requires
        /// </summary>
        ICollection<MetadataReference> AssembliesUsing { get; }

        /// <summary>
        /// List of interfaces that need to be injected by this aspect
        /// </summary>
        ICollection<Type> InterfacesUsing { get; }

        /// <summary>
        /// Using statements that the aspect requires
        /// </summary>
        ICollection<string> Usings { get; }

        /// <summary>
        /// Used to hook into the object once it has been created
        /// </summary>
        /// <param name="value">Object created by the system</param>
        void Setup(object value);

        /// <summary>
        /// Used to insert code into the default constructor
        /// </summary>
        /// <param name="baseType">Base type</param>
        /// <returns>The code to insert</returns>
        string SetupDefaultConstructor(Type baseType);

        /// <summary>
        /// Used to insert code at the end of the method
        /// </summary>
        /// <param name="method">Overridding Method</param>
        /// <param name="baseType">Base type</param>
        /// <param name="returnValueName">Local holder for the value returned by the function</param>
        /// <returns>The code to insert</returns>
        string SetupEndMethod(MethodInfo method, Type baseType, string returnValueName);

        /// <summary>
        /// Used to insert code within the catch portion of the try/catch portion of the method
        /// </summary>
        /// <param name="method">Overridding Method</param>
        /// <param name="baseType">Base type</param>
        /// <returns>The code to insert</returns>
        string SetupExceptionMethod(MethodInfo method, Type baseType);

        /// <summary>
        /// Used to set up any interfaces, extra fields, methods, etc. prior to overridding any methods.
        /// </summary>
        /// <param name="type">Type of the object</param>
        /// <returns>The code to insert</returns>
        string SetupInterfaces(Type type);

        /// <summary>
        /// Used to insert code at the beginning of the method
        /// </summary>
        /// <param name="method">Overridding Method</param>
        /// <param name="baseType">Base type</param>
        /// <returns>The code to insert</returns>
        string SetupStartMethod(MethodInfo method, Type baseType);
    }
}