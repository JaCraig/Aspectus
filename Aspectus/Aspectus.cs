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
using Fast.Activator;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        /// Initializes a new instance of the <see cref="Aspectus"/> class.
        /// </summary>
        /// <param name="compiler">The compiler.</param>
        /// <param name="aspects">The aspects.</param>
        /// <param name="modules">The modules.</param>
        /// <param name="logger">Serilog based log object</param>
        /// <param name="objectPool">The object pool.</param>
        /// <exception cref="ArgumentNullException">logger</exception>
        public Aspectus(Compiler compiler, IEnumerable<IAspect> aspects, IEnumerable<IAOPModule> modules, ILogger<Aspectus>? logger, ObjectPool<StringBuilder>? objectPool)
        {
            Logger = logger;
            aspects ??= Array.Empty<IAspect>();
            modules ??= Array.Empty<IAOPModule>();
            Compiler = compiler ?? new Compiler(objectPool);
            if (Aspects.IsEmpty)
                Aspects.Add(aspects);
            modules.ForEachParallel(x => x.Setup(this));
            ObjectPool = objectPool;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Aspectus"/> class.
        /// </summary>
        /// <param name="compiler">The compiler.</param>
        /// <param name="aspects">The aspects.</param>
        /// <param name="modules">The modules.</param>
        public Aspectus(Compiler compiler, IEnumerable<IAspect> aspects, IEnumerable<IAOPModule> modules)
            : this(compiler, aspects, modules, null, null)
        {
        }

        /// <summary>
        /// The list of aspects that are being used
        /// </summary>
        private ConcurrentBag<IAspect> Aspects { get; } = new ConcurrentBag<IAspect>();

        /// <summary>
        /// Gets the system's compiler
        /// </summary>
        private Compiler? Compiler { get; set; }

        /// <summary>
        /// Logging object
        /// </summary>
        private ILogger? Logger { get; }

        /// <summary>
        /// Gets the object pool.
        /// </summary>
        /// <value>The object pool.</value>
        private ObjectPool<StringBuilder>? ObjectPool { get; }

        /// <summary>
        /// Dictionary containing generated types and associates it with original type
        /// </summary>
        private readonly ConcurrentDictionary<Type, Type> _Classes = new();

        /// <summary>
        /// Creates an object of the specified base type, registering the type if necessary
        /// </summary>
        /// <typeparam name="T">The base type</typeparam>
        /// <returns>Returns an object of the specified base type</returns>
        public T Create<T>() => (T)Create(typeof(T))!;

        /// <summary>
        /// Creates an object of the specified base type, registering the type if necessary
        /// </summary>
        /// <param name="baseType">The base type</param>
        /// <returns>Returns an object of the specified base type</returns>
        public object? Create(Type baseType)
        {
            if (baseType is null)
                return null;
            if (!_Classes.ContainsKey(baseType))
                Setup(baseType);
            if (!_Classes.ContainsKey(baseType) || _Classes[baseType] is null)
                return GetInstance(baseType);
            var ReturnObject = GetInstance(_Classes[baseType]);
            if (ReturnObject is null)
                return ReturnObject;
            if (_Classes[baseType] != baseType)
            {
                foreach (IAspect Aspect in Aspects)
                {
                    Aspect.Setup(ReturnObject);
                }
            }
            return ReturnObject;
        }

        /// <summary>
        /// Cleans up items in this instance.
        /// </summary>
        public void FinalizeSetup()
        {
            Compiler?.Dispose();
            Compiler = null;
        }

        /// <summary>
        /// Sets up all types from the assembly that it can
        /// </summary>
        /// <param name="assemblies">Assembly to set up</param>
        public void Setup(params Assembly[] assemblies)
        {
            if (assemblies is null || assemblies.Length == 0)
                return;
            Setup(FilterTypesToSetup(assemblies.SelectMany(x => x.ExportedTypes)));
        }

        /// <summary>
        /// Sets up a type so it can be used in the system later
        /// </summary>
        /// <param name="types">The types.</param>
        public void Setup(params Type[] types)
        {
            if (Compiler is null)
                return;
            IEnumerable<Type> TempTypes = FilterTypesToSetup(types);
            if (!TempTypes.Any())
            {
                for (var x = 0; x < types.Length; ++x)
                {
                    _Classes.AddOrUpdate(types[x], types[x], (y, _) => y);
                }
                return;
            }
            var TempAssemblies = new List<Assembly>();
            GetAssemblies(typeof(object), TempAssemblies);
            GetAssemblies(typeof(Enumerable), TempAssemblies);

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
            Aspects.ForEach(x => InterfacesUsed.AddRange(x.InterfacesUsing ?? Array.Empty<Type>()));

            StringBuilder Builder = ObjectPool?.Get() ?? new StringBuilder();

            foreach (Type TempType in TempTypes)
            {
                Logger?.LogDebug("Generating type for {0}", TempType.GetName(ObjectPool));
                GetAssemblies(TempType, TempAssemblies);

                var Namespace = "AspectusGeneratedTypes.C" + Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
                var ClassName = TempType.Name + "Derived";
                Builder.AppendLine(Setup(TempType, Namespace, ClassName, Usings, InterfacesUsed, TempAssemblies));
            }
            try
            {
                List<MetadataReference> MetadataReferences = GetFinalAssemblies(TempAssemblies);
                Aspects.ForEach(x => MetadataReferences.AddIfUnique((z, y) => z.Display == y.Display, x.AssembliesUsing.ToArray()));
                IEnumerable<Type> Types = Compiler.Create(Builder.ToString(), Usings, MetadataReferences.ToArray())
                                                    .Compile()
                                                    .LoadAssembly();
                foreach (Type TempType in TempTypes)
                {
                    _Classes.AddOrUpdate(TempType,
                        Types.FirstOrDefault(x => x.BaseType == TempType),
                        (x, _) => x);
                }
            }
            catch (Exception Ex)
            {
                Logger?.LogError(Ex, "Error compiling code");
                foreach (Type TempType in TempTypes)
                {
                    _Classes.AddOrUpdate(TempType,
                        TempType,
                        (x, _) => x);
                }
            }
            ObjectPool?.Return(Builder);
        }

        /// <summary>
        /// Outputs manager info as a string
        /// </summary>
        /// <returns>String version of the manager</returns>
        public override string ToString() => "AOP registered classes: " + _Classes.Keys.ToString(x => x.Name) + "\r\n";

        /// <summary>
        /// Gets the assemblies.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="assembliesUsing">The assemblies using.</param>
        private static void GetAssemblies(Type type, List<Assembly> assembliesUsing)
        {
            Type? TempType = type;
            while (TempType != null)
            {
                assembliesUsing.AddIfUnique(TempType.Assembly);
                TempType.GetInterfaces().ForEach(x => GetAssembliesSimple(x, assembliesUsing));
                TempType.GetEvents().ForEach(x => GetAssembliesSimple(x.EventHandlerType, assembliesUsing));
                TempType.GetFields().ForEach(x => GetAssembliesSimple(x.FieldType, assembliesUsing));
                TempType.GetProperties().ForEach(x => GetAssembliesSimple(x.PropertyType, assembliesUsing));
                TempType.GetMethods().ForEach(x =>
                {
                    GetAssembliesSimple(x.ReturnType, assembliesUsing);
                    x.GetParameters().ForEach(y => GetAssembliesSimple(y.ParameterType, assembliesUsing));
                });
                TempType = TempType.BaseType;
                if (TempType == typeof(object))
                    break;
            }
        }

        /// <summary>
        /// Gets the assemblies simple.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="assembliesUsing">The assemblies using.</param>
        private static void GetAssembliesSimple(Type type, List<Assembly> assembliesUsing)
        {
            Type? TempType = type;
            while (TempType != null)
            {
                assembliesUsing.AddIfUnique((z, y) => z.Location == y.Location, TempType.Assembly);
                TempType.GetInterfaces().ForEach(x => GetAssembliesSimple(x, assembliesUsing));
                TempType = TempType.BaseType;
                if (TempType == typeof(object))
                    break;
            }
        }

        /// <summary>
        /// Gets the final assemblies.
        /// </summary>
        /// <param name="assembliesUsing">The assemblies using.</param>
        /// <returns>The final assemblies</returns>
        private static List<MetadataReference> GetFinalAssemblies(List<Assembly> assembliesUsing)
        {
            assembliesUsing.AddIfUnique((z, y) => z.Location == y.Location, assembliesUsing
                                                    .SelectMany(x => x.GetReferencedAssemblies())
                                                    .Select(Assembly.Load).ToArray());
            string[] Load = {
                "mscorlib.dll",
                "netstandard.dll"
            };
            var ReturnList = assembliesUsing.ForEach(x => (MetadataReference)MetadataReference.CreateFromFile(x.Location))
                                  .ToList();
            foreach (var Directory in assembliesUsing.Select(x => new FileInfo(x.Location).DirectoryName).Distinct())
            {
                foreach (FileInfo? DLL in new DirectoryInfo(Directory)
                                            .EnumerateFiles("*.dll")
                                            .Where(x => Load.Contains(x.Name)))
                {
                    PortableExecutableReference TempAssembly = MetadataReference.CreateFromFile(DLL.FullName);
                    ReturnList.AddIfUnique((z, y) => z.Display == y.Display, TempAssembly);
                }
            }
            return ReturnList;
        }

        /// <summary>
        /// Gets an instance of the object.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The object.</returns>
        private static object? GetInstance(Type type) => FastActivator.CreateInstance(type);

        /// <summary>
        /// Determines whether this instance can setup the specified types.
        /// </summary>
        /// <param name="enumerable">The list of types</param>
        /// <returns>The types that can be set up</returns>
        private Type[] FilterTypesToSetup(IEnumerable<Type> enumerable)
        {
            enumerable ??= Array.Empty<Type>();
            return enumerable.Where(x =>
            {
                Type TempTypeInfo = x;
                return !_Classes.ContainsKey(x)
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
        private string Setup(Type type,
            string namespaceUsing,
            string className,
            List<string> usings,
            List<Type> interfaces,
            List<Assembly> assembliesUsing)
        {
            StringBuilder Builder = ObjectPool?.Get() ?? new StringBuilder();
            Builder.AppendLineFormat(@"namespace {1}
{{
    {0}

    public class {2} : {3}{4} {5}
    {{
", usings.ToString(x => "using " + x + ";", "\r\n"),
 namespaceUsing,
 className,
 type.FullName.Replace("+", ".", StringComparison.Ordinal),
 interfaces.Count > 0 ? "," : string.Empty, interfaces.ToString(x => x.FullName.Replace("+", ".", StringComparison.Ordinal)));
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

            Type? TempType = type;
            var MethodsAlreadyDone = new List<string>();
            while (TempType != null)
            {
                for (int i = 0, MaxLength = TempType.GetProperties().Length; i < MaxLength; i++)
                {
                    PropertyInfo Property = TempType.GetProperties()[i];
                    MethodInfo? GetMethodInfo = Property.GetMethod;
                    MethodInfo? SetMethodInfo = Property.SetMethod;
                    if (!MethodsAlreadyDone.Contains("get_" + Property.Name)
                        && !MethodsAlreadyDone.Contains("set_" + Property.Name)
                        && GetMethodInfo?.IsVirtual == true
                        && SetMethodInfo?.IsPublic == true
                        && !GetMethodInfo.IsFinal
                        && Property.GetIndexParameters().Length == 0)
                    {
                        GetAssemblies(Property.PropertyType, assembliesUsing);
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
                                                    Property.PropertyType.GetName(ObjectPool),
                                                    Property.Name,
                                                    SetupMethod(type, GetMethodInfo, true),
                                                    SetupMethod(type, SetMethodInfo, true));
                        MethodsAlreadyDone.Add(GetMethodInfo.Name);
                        MethodsAlreadyDone.Add(SetMethodInfo.Name);
                    }
                    else if (!MethodsAlreadyDone.Contains("get_" + Property.Name)
                        && GetMethodInfo?.IsVirtual == true
                        && SetMethodInfo is null
                        && !GetMethodInfo.IsFinal
                        && Property.GetIndexParameters().Length == 0)
                    {
                        GetAssemblies(Property.PropertyType, assembliesUsing);
                        Builder.AppendLineFormat(@"
        public override {0} {1}
        {{
            get
            {{
                {2}
            }}
        }}",
                                                    Property.PropertyType.GetName(ObjectPool),
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

                for (int i = 0, MaxLength = TempType.GetMethods().Length; i < MaxLength; i++)
                {
                    MethodInfo Method = TempType.GetMethods()[i];
                    const string MethodAttribute = "public";
                    if (!MethodsAlreadyDone.Contains(Method.Name)
                        && Method.IsVirtual
                        && !Method.IsFinal
                        && !Method.IsPrivate
                        && !Method.Name.StartsWith("add_", StringComparison.OrdinalIgnoreCase)
                        && !Method.Name.StartsWith("remove_", StringComparison.OrdinalIgnoreCase)
                        && !Method.IsGenericMethod)
                    {
                        GetAssemblies(Method.ReturnType, assembliesUsing);
                        Method.GetParameters().ForEach(x => GetAssemblies(x.ParameterType, assembliesUsing));
                        var Static = Method.IsStatic ? "static " : string.Empty;
                        Builder.AppendLineFormat(@"
        {4} override {0} {1}({2})
        {{
            {3}
        }}",
                                                    Static + Method.ReturnType.GetName(ObjectPool),
                                                    Method.Name,
                                                    Method.GetParameters().ToString(x => (x.IsOut ? "out " : string.Empty) + x.ParameterType.GetName(ObjectPool) + " " + x.Name),
                                                    SetupMethod(type, Method, false),
                                                    MethodAttribute);
                        MethodsAlreadyDone.Add(Method.Name);
                    }
                }

                TempType = TempType.BaseType;
                if (TempType == typeof(object))
                    break;
            }
            Builder.AppendLine(@"   }
}");
            var ReturnValue = Builder.ToString();
            ObjectPool?.Return(Builder);
            return ReturnValue;
        }

        private string SetupMethod(Type type, MethodInfo methodInfo, bool isProperty)
        {
            if (methodInfo is null)
                return string.Empty;
            StringBuilder Builder = ObjectPool?.Get() ?? new StringBuilder();
            var BaseMethodName = methodInfo.Name.Replace("get_", string.Empty, StringComparison.Ordinal).Replace("set_", string.Empty, StringComparison.Ordinal);
            var ReturnValue = methodInfo.ReturnType != typeof(void) ? "FinalReturnValue" : string.Empty;
            var BaseCall = string.Empty;
            BaseCall = isProperty
                ? string.IsNullOrEmpty(ReturnValue) ? "base." + BaseMethodName : ReturnValue + "=base." + BaseMethodName
                : string.IsNullOrEmpty(ReturnValue) ? "base." + BaseMethodName + "(" : ReturnValue + "=base." + BaseMethodName + "(";
            ParameterInfo[] Parameters = methodInfo.GetParameters();
            if (isProperty)
            {
                BaseCall += Parameters.Length > 0 ? "=" + Parameters.ToString(x => x.Name) + ";" : ";";
            }
            else
            {
                BaseCall += Parameters.Length > 0 ? Parameters.ToString(x => (x.IsOut ? "out " : string.Empty) + x.Name) : string.Empty;
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
                methodInfo.ReturnType != typeof(void) ? methodInfo.ReturnType.GetName(ObjectPool) + " " + ReturnValue + ";" : string.Empty,
                Aspects.ForEach(x => x.SetupStartMethod(methodInfo, type)).ToString(x => x, "\r\n"),
                BaseCall,
                Aspects.ForEach(x => x.SetupEndMethod(methodInfo, type, ReturnValue)).ToString(x => x, "\r\n"),
                string.IsNullOrEmpty(ReturnValue) ? string.Empty : "return " + ReturnValue + ";",
                Aspects.ForEach(x => x.SetupExceptionMethod(methodInfo, type)).ToString(x => x, "\r\n"));
            var ReturnVal = Builder.ToString();
            ObjectPool?.Return(Builder);
            return ReturnVal;
        }
    }
}