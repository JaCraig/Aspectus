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

using Aspectus.CodeGen;
using Aspectus.HelperFunctions;
using Aspectus.Interfaces;
using Microsoft.CodeAnalysis;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Aspectus
{
    /// <summary>
    /// AOP interface manager
    /// </summary>
    public class Aspectus
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="compiler">The compiler.</param>
        /// <param name="aspects">The aspects.</param>
        /// <param name="modules">The modules.</param>
        /// <param name="logger">Serilog based log object</param>
        public Aspectus(Compiler compiler, IEnumerable<IAspect> aspects, IEnumerable<IAOPModule> modules, ILogger logger)
        {
            Logger = logger ?? Log.Logger ?? new LoggerConfiguration().CreateLogger() ?? throw new ArgumentNullException(nameof(logger));
            aspects = aspects ?? new List<IAspect>();
            modules = modules ?? new List<IAOPModule>();
            Compiler = compiler ?? new Compiler(Logger);
            if (Aspects.Count == 0)
                Aspects.Add(aspects);
            modules.ForEachParallel(x => x.Setup(this));
        }

        /// <summary>
        /// The list of aspects that are being used
        /// </summary>
        private readonly ConcurrentBag<IAspect> Aspects = new ConcurrentBag<IAspect>();

        /// <summary>
        /// Dictionary containing generated types and associates it with original type
        /// </summary>
        private ConcurrentDictionary<Type, Type> Classes = new ConcurrentDictionary<Type, Type>();

        /// <summary>
        /// Gets the system's compiler
        /// </summary>
        protected static Compiler Compiler { get; private set; }

        /// <summary>
        /// Logging object
        /// </summary>
        private ILogger Logger { get; }

        /// <summary>
        /// Creates an object of the specified base type, registering the type if necessary
        /// </summary>
        /// <typeparam name="T">The base type</typeparam>
        /// <returns>Returns an object of the specified base type</returns>
        public T Create<T>()
        {
            return (T)Create(typeof(T));
        }

        /// <summary>
        /// Creates an object of the specified base type, registering the type if necessary
        /// </summary>
        /// <param name="baseType">The base type</param>
        /// <returns>Returns an object of the specified base type</returns>
        public object Create(Type baseType)
        {
            if (!Classes.ContainsKey(baseType))
                Setup(baseType);
            if (!Classes.ContainsKey(baseType))
                return Activator.CreateInstance(baseType);
            var ReturnObject = Activator.CreateInstance(Classes[baseType]);
            if (Classes[baseType] != baseType)
                Aspects.ForEach(x => x.Setup(ReturnObject));
            return ReturnObject;
        }

        /// <summary>
        /// Sets up all types from the assembly that it can
        /// </summary>
        /// <param name="assemblies">Assembly to set up</param>
        public void Setup(params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
                return;
            Setup(FilterTypesToSetup(assemblies.SelectMany(x => x.ExportedTypes)));
        }

        /// <summary>
        /// Sets up a type so it can be used in the system later
        /// </summary>
        /// <param name="types">The types.</param>
        public void Setup(params Type[] types)
        {
            IEnumerable<Type> TempTypes = FilterTypesToSetup(types);
            if (!TempTypes.Any())
                return;
            var AssemblyPath = System.IO.Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location);
            var AssembliesUsing = new List<MetadataReference>();
            AssembliesUsing.AddIfUnique(MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location));
            AssembliesUsing.AddIfUnique(MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.Location));
            Aspects.ForEach(x => AssembliesUsing.AddIfUnique(x.AssembliesUsing));

            var Usings = new List<string>
            {
                "System",
                "System.Collections.Generic",
                "System.Linq",
                "System.Text",
                "System.Threading.Tasks"
            };
            Aspects.ForEach(x => Usings.AddIfUnique(x.Usings));

            var InterfacesUsed = new List<Type>();
            Aspects.ForEach(x => InterfacesUsed.AddRange(x.InterfacesUsing ?? new List<Type>()));

            var Builder = new StringBuilder();

            foreach (Type TempType in TempTypes)
            {
                Logger.Debug("Generating type for {Info:l}", TempType.GetName());
                AssembliesUsing.AddIfUnique(MetadataReference.CreateFromFile(TempType.GetTypeInfo().Assembly.Location));
                AssembliesUsing.AddIfUnique(GetAssemblies(TempType).ForEach(y => MetadataReference.CreateFromFile(y.Location)));

                string Namespace = "AspectusGeneratedTypes.C" + Guid.NewGuid().ToString("N");
                string ClassName = TempType.Name + "Derived";
                Builder.AppendLine(Setup(TempType, Namespace, ClassName, Usings, InterfacesUsed, AssembliesUsing));
            }
            try
            {
                var Types = Compiler.Create(Builder.ToString(), Usings, AssembliesUsing.ToArray())
                                                    .Compile()
                                                    .LoadAssembly();
                foreach (Type TempType in TempTypes)
                {
                    Classes.AddOrUpdate(TempType,
                        Types.FirstOrDefault(x => x.GetTypeInfo().BaseType == TempType),
                        (x, y) => x);
                }
            }
            catch (Exception)
            {
                foreach (Type TempType in TempTypes)
                {
                    Classes.AddOrUpdate(TempType,
                        TempType,
                        (x, y) => x);
                }
            }
        }

        /// <summary>
        /// Outputs manager info as a string
        /// </summary>
        /// <returns>String version of the manager</returns>
        public override string ToString()
        {
            return "AOP registered classes: " + Classes.Keys.ToString(x => x.Name) + "\r\n";
        }

        private static Assembly[] GetAssemblies(Type type)
        {
            var Types = new List<Assembly>();
            Type TempType = type;
            while (TempType != null)
            {
                Types.AddIfUnique(TempType.GetTypeInfo().Assembly);
                TempType.GetTypeInfo().ImplementedInterfaces.ForEach(x => Types.AddIfUnique(GetAssembliesSimple(x)));
                TempType.GetTypeInfo().DeclaredEvents.ForEach(x => Types.AddIfUnique(GetAssembliesSimple(x.EventHandlerType)));
                TempType.GetTypeInfo().DeclaredFields.ForEach(x => Types.AddIfUnique(GetAssembliesSimple(x.FieldType)));
                TempType.GetTypeInfo().DeclaredProperties.ForEach(x => Types.AddIfUnique(GetAssembliesSimple(x.PropertyType)));
                TempType.GetTypeInfo().DeclaredMethods.ForEach(x =>
                {
                    Types.AddIfUnique(GetAssembliesSimple(x.ReturnType));
                    x.GetParameters().ForEach(y => Types.AddIfUnique(GetAssembliesSimple(y.ParameterType)));
                });
                TempType = TempType.GetTypeInfo().BaseType;
                if (TempType == typeof(object))
                    break;
            }
            return Types.ToArray();
        }

        private static Assembly[] GetAssembliesSimple(Type type)
        {
            var Types = new List<Assembly>();
            Type TempType = type;
            while (TempType != null)
            {
                Types.AddIfUnique(TempType.GetTypeInfo().Assembly);
                TempType.GetTypeInfo().ImplementedInterfaces.ForEach(x => Types.AddIfUnique(GetAssembliesSimple(x)));
                TempType = TempType.GetTypeInfo().BaseType;
                if (TempType == typeof(object))
                    break;
            }
            return Types.ToArray();
        }

        /// <summary>
        /// Determines whether this instance can setup the specified types.
        /// </summary>
        /// <param name="enumerable">The list of types</param>
        /// <returns>The types that can be set up</returns>
        private Type[] FilterTypesToSetup(IEnumerable<Type> enumerable)
        {
            enumerable = enumerable ?? new List<Type>();
            return enumerable.Where(x =>
            {
                var TempTypeInfo = x.GetTypeInfo();
                return !Classes.ContainsKey(x)
                        && !TempTypeInfo.ContainsGenericParameters
                        && TempTypeInfo.IsClass
                        && (TempTypeInfo.IsPublic || TempTypeInfo.IsNestedPublic)
                        && !TempTypeInfo.IsSealed
                        && TempTypeInfo.IsVisible
                        && x.HasDefaultConstructor()
                        && !string.IsNullOrEmpty(x.Namespace);
            })
            .ToArray();
        }

        /// <summary>
        /// Setups the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="namespaceUsing">The namespace.</param>
        /// <param name="className">Name of the class.</param>
        /// <param name="usings">The usings.</param>
        /// <param name="interfaces">The interfaces.</param>
        /// <param name="assembliesUsing">The assemblies using.</param>
        /// <returns></returns>
        private string Setup(Type type, string namespaceUsing,
            string className, List<string> usings,
            List<Type> interfaces, List<MetadataReference> assembliesUsing)
        {
            var Builder = new StringBuilder();
            Builder.AppendLineFormat(@"namespace {1}
{{
    {0}

    public class {2} : {3}{4} {5}
    {{
", usings.ToString(x => "using " + x + ";", "\r\n"),
 namespaceUsing,
 className,
 type.FullName.Replace("+", "."),
 interfaces.Count > 0 ? "," : "", interfaces.ToString(x => x.FullName.Replace("+", ".")));
            if (type.HasDefaultConstructor())
            {
                Builder.AppendLineFormat(@"
        public {0}()
            :base()
        {{
            ",
               type.Name + "Derived");
                Aspects.ForEach(x => Builder.AppendLine(x.SetupDefaultConstructor(type)));
                Builder.AppendLineFormat(@"
        }}");
            }

            Aspects.ForEach(x => Builder.AppendLine(x.SetupInterfaces(type)));

            Type TempType = type;
            var MethodsAlreadyDone = new List<string>();
            while (TempType != null)
            {
                foreach (PropertyInfo Property in TempType.GetTypeInfo().DeclaredProperties)
                {
                    MethodInfo GetMethodInfo = Property.GetMethod;
                    MethodInfo SetMethodInfo = Property.SetMethod;
                    if (!MethodsAlreadyDone.Contains("get_" + Property.Name)
                        && !MethodsAlreadyDone.Contains("set_" + Property.Name)
                        && GetMethodInfo != null
                        && GetMethodInfo.IsVirtual
                        && SetMethodInfo != null
                        && SetMethodInfo.IsPublic
                        && !GetMethodInfo.IsFinal
                        && Property.GetIndexParameters().Length == 0)
                    {
                        assembliesUsing.AddIfUnique(GetAssemblies(Property.PropertyType).ForEach(y => MetadataReference.CreateFromFile(y.Location)));
                        Builder.AppendLineFormat(@"
        public override {0} {1}
        {{
            get
            {{
                {2}
            }}
            set
            {{
                {3}
            }}
        }}",
                                                    Property.PropertyType.GetName(),
                                                    Property.Name,
                                                    SetupMethod(type, GetMethodInfo, true),
                                                    SetupMethod(type, SetMethodInfo, true));
                        MethodsAlreadyDone.Add(GetMethodInfo.Name);
                        MethodsAlreadyDone.Add(SetMethodInfo.Name);
                    }
                    else if (!MethodsAlreadyDone.Contains("get_" + Property.Name)
                        && GetMethodInfo != null
                        && GetMethodInfo.IsVirtual
                        && SetMethodInfo == null
                        && !GetMethodInfo.IsFinal
                        && Property.GetIndexParameters().Length == 0)
                    {
                        assembliesUsing.AddIfUnique(GetAssemblies(Property.PropertyType).ForEach(y => MetadataReference.CreateFromFile(y.Location)));
                        Builder.AppendLineFormat(@"
        public override {0} {1}
        {{
            get
            {{
                {2}
            }}
        }}",
                                                    Property.PropertyType.GetName(),
                                                    Property.Name,
                                                    SetupMethod(type, GetMethodInfo, true));
                        MethodsAlreadyDone.Add(GetMethodInfo.Name);
                    }
                    else
                    {
                        if (GetMethodInfo != null)
                            MethodsAlreadyDone.Add(GetMethodInfo.Name);
                        if (SetMethodInfo != null)
                            MethodsAlreadyDone.Add(SetMethodInfo.Name);
                    }
                }
                foreach (MethodInfo Method in TempType.GetTypeInfo().DeclaredMethods)
                {
                    string MethodAttribute = "public";
                    if (!MethodsAlreadyDone.Contains(Method.Name)
                        && Method.IsVirtual
                        && !Method.IsFinal
                        && !Method.IsPrivate
                        && !Method.Name.StartsWith("add_", StringComparison.OrdinalIgnoreCase)
                        && !Method.Name.StartsWith("remove_", StringComparison.OrdinalIgnoreCase)
                        && !Method.IsGenericMethod)
                    {
                        assembliesUsing.AddIfUnique(GetAssemblies(Method.ReturnType).ForEach(y => MetadataReference.CreateFromFile(y.Location)));
                        Method.GetParameters().ForEach(x => assembliesUsing.AddIfUnique(GetAssemblies(x.ParameterType).ForEach(y => MetadataReference.CreateFromFile(y.Location))));
                        string Static = Method.IsStatic ? "static " : "";
                        Builder.AppendLineFormat(@"
        {4} override {0} {1}({2})
        {{
            {3}
        }}",
                                                    Static + Method.ReturnType.GetName(),
                                                    Method.Name,
                                                    Method.GetParameters().ToString(x => (x.IsOut ? "out " : "") + x.ParameterType.GetName() + " " + x.Name),
                                                    SetupMethod(type, Method, false),
                                                    MethodAttribute);
                        MethodsAlreadyDone.Add(Method.Name);
                    }
                }
                TempType = TempType.GetTypeInfo().BaseType;
                if (TempType == typeof(object))
                    break;
            }
            Builder.AppendLine(@"   }
}");
            return Builder.ToString();
        }

        private string SetupMethod(Type type, MethodInfo methodInfo, bool isProperty)
        {
            if (methodInfo == null)
                return "";
            var Builder = new StringBuilder();
            var BaseMethodName = methodInfo.Name.Replace("get_", "").Replace("set_", "");
            string ReturnValue = methodInfo.ReturnType != typeof(void) ? "FinalReturnValue" : "";
            string BaseCall = "";
            if (isProperty)
                BaseCall = string.IsNullOrEmpty(ReturnValue) ? "base." + BaseMethodName : ReturnValue + "=base." + BaseMethodName;
            else
                BaseCall = string.IsNullOrEmpty(ReturnValue) ? "base." + BaseMethodName + "(" : ReturnValue + "=base." + BaseMethodName + "(";
            var Parameters = methodInfo.GetParameters();
            if (isProperty)
            {
                BaseCall += Parameters.Length > 0 ? "=" + Parameters.ToString(x => x.Name) + ";" : ";";
            }
            else
            {
                BaseCall += Parameters.Length > 0 ? Parameters.ToString(x => (x.IsOut ? "out " : "") + x.Name) : "";
                BaseCall += ");\r\n";
            }
            Builder.AppendLineFormat(@"
                try
                {{
                    {0}
                    {1}
                    {2}
                    {3}
                    {4}
                }}
                catch(Exception CaughtException)
                {{
                    {5}
                    throw;
                }}",
                methodInfo.ReturnType != typeof(void) ? methodInfo.ReturnType.GetName() + " " + ReturnValue + ";" : "",
                Aspects.ForEach(x => x.SetupStartMethod(methodInfo, type)).ToString(x => x, "\r\n"),
                BaseCall,
                Aspects.ForEach(x => x.SetupEndMethod(methodInfo, type, ReturnValue)).ToString(x => x, "\r\n"),
                string.IsNullOrEmpty(ReturnValue) ? "" : "return " + ReturnValue + ";",
                Aspects.ForEach(x => x.SetupExceptionMethod(methodInfo, type)).ToString(x => x, "\r\n"));
            return Builder.ToString();
        }
    }
}