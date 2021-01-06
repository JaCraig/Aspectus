# Aspectus

[![Build status](https://ci.appveyor.com/api/projects/status/j5968toe7ikmj826?svg=true)](https://ci.appveyor.com/project/JaCraig/aspectus)

Aspectus is an AOP library that allows you to inject cross cutting concerns in an easy manner.


## Setting Up the Library

Aspectus relies on [Canister](https://github.com/JaCraig/Canister) in order to hook itself up. In order for this to work, you must do the following at startup:

    services.AddCanisterModules(configure => configure.RegisterAspectus());
					
The RegisterAspectus function is an extension method that registers it with the IoC container. When this is done, Aspectus is ready to use.

## Basic Usage

The way that Aspectus handles AOP is by doing code generation using Roslyn. As such you're writing C# codes for the most part. Start by implementing a class that inherits from IAspect:

    public interface IExample
    {
        string MySecretData { get; set; }
    }

    public class TestAspect : IAspect
    {
        public TestAspect()
        {
            AssembliesUsing = new List<MetadataReference>();
            AssembliesUsing.Add(MetadataReference.CreateFromFile(typeof(TestAspect).GetTypeInfo().Assembly.Location));
            foreach (var DLL in new DirectoryInfo(@"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\1.0.0\")
                                                        .EnumerateFiles("*.dll")
                                                        .Where(x => !DontLoad.Contains(x.Name)))
            {
                var TempAssembly = MetadataReference.CreateFromFile(DLL.FullName);
                AssembliesUsing.Add(TempAssembly);
            }
        }

        public ICollection<MetadataReference> AssembliesUsing { get; private set; }

        public ICollection<Type> InterfacesUsing => new Type[] { typeof(IExample) };

        public ICollection<string> Usings => new string[] { };

        private string[] DontLoad = {
            "sos.dll",
            "mscorrc.dll",
            "mscorrc.debug.dll",
            "mscordbi.dll",
            "mscordaccore.dll",
            "libuv.dll",
            "hostpolicy.dll",
            "hostfxr.dll",
            "ext-ms-win-ntuser-keyboard-l1-2-1.dll",
            "ext-ms-win-advapi32-encryptedfile-l1-1-0.dll",
            "dbgshim.dll",
            "coreclr.dll",
            "clrjit.dll",
            "clretwrc.dll",
            "clrcompression.dll",
            "api-ms-win-service-winsvc-l1-1-0.dll",
            "api-ms-win-service-private-l1-1-1.dll",
            "api-ms-win-service-private-l1-1-0.dll",
            "api-ms-win-service-management-l2-1-0.dll",
            "api-ms-win-service-management-l1-1-0.dll",
            "api-ms-win-service-core-l1-1-1.dll",
            "api-ms-win-service-core-l1-1-0.dll",
            "api-ms-win-security-sddl-l1-1-0.dll",
            "api-ms-win-security-provider-l1-1-0.dll",
            "API-MS-Win-Security-LsaPolicy-L1-1-0.dll",
            "api-ms-win-security-lsalookup-l2-1-1.dll",
            "api-ms-win-security-lsalookup-l2-1-0.dll",
            "api-ms-win-security-cryptoapi-l1-1-0.dll",
            "api-ms-win-security-cpwl-l1-1-0.dll",
            "api-ms-win-security-base-l1-1-0.dll",
            "api-ms-win-ro-typeresolution-l1-1-0.dll",
            "API-MS-Win-EventLog-Legacy-L1-1-0.dll",
            "API-MS-Win-Eventing-Provider-L1-1-0.dll",
            "API-MS-Win-Eventing-Legacy-L1-1-0.dll",
            "API-MS-Win-Eventing-Controller-L1-1-0.dll",
            "API-MS-Win-Eventing-Consumer-L1-1-0.dll",
            "API-MS-Win-Eventing-ClassicProvider-L1-1-0.dll",
            "API-MS-Win-devices-config-L1-1-1.dll",
            "API-MS-Win-devices-config-L1-1-0.dll",
            "api-ms-win-core-xstate-l2-1-0.dll",
            "api-ms-win-core-xstate-l1-1-0.dll",
            "api-ms-win-core-wow64-l1-1-0.dll",
            "api-ms-win-core-winrt-string-l1-1-0.dll",
            "api-ms-win-core-winrt-roparameterizediid-l1-1-0.dll",
            "api-ms-win-core-winrt-robuffer-l1-1-0.dll",
            "api-ms-win-core-winrt-registration-l1-1-0.dll",
            "api-ms-win-core-winrt-l1-1-0.dll",
            "api-ms-win-core-winrt-error-l1-1-1.dll",
            "api-ms-win-core-winrt-error-l1-1-0.dll",
            "api-ms-win-core-version-l1-1-0.dll",
            "api-ms-win-core-util-l1-1-0.dll",
            "api-ms-win-core-url-l1-1-0.dll",
            "api-ms-win-core-timezone-l1-1-0.dll",
            "api-ms-win-core-threadpool-private-l1-1-0.dll",
            "api-ms-win-core-threadpool-legacy-l1-1-0.dll",
            "api-ms-win-core-threadpool-l1-2-0.dll",
            "api-ms-win-core-sysinfo-l1-2-3.dll",
            "api-ms-win-core-sysinfo-l1-2-2.dll",
            "api-ms-win-core-sysinfo-l1-2-1.dll",
            "api-ms-win-core-sysinfo-l1-2-0.dll",
            "api-ms-win-core-sysinfo-l1-1-0.dll",
            "api-ms-win-core-synch-l1-2-0.dll",
            "api-ms-win-core-synch-l1-1-0.dll",
            "api-ms-win-core-stringloader-l1-1-1.dll",
            "api-ms-win-core-stringloader-l1-1-0.dll",
            "API-MS-Win-Core-StringAnsi-L1-1-0.dll",
            "api-ms-win-core-string-obsolete-l1-1-1.dll",
            "api-ms-win-core-string-obsolete-l1-1-0.dll",
            "API-MS-Win-Core-String-L2-1-0.dll",
            "api-ms-win-core-string-l1-1-0.dll",
            "api-ms-win-core-shutdown-l1-1-1.dll",
            "api-ms-win-core-shutdown-l1-1-0.dll",
            "api-ms-win-core-shlwapi-obsolete-l1-1-0.dll",
            "api-ms-win-core-shlwapi-legacy-l1-1-0.dll",
            "api-ms-win-core-rtlsupport-l1-1-0.dll",
            "api-ms-win-core-registry-l2-1-0.dll",
            "api-ms-win-core-registry-l1-1-0.dll",
            "api-ms-win-core-realtime-l1-1-0.dll",
            "api-ms-win-core-psapi-obsolete-l1-1-0.dll",
            "api-ms-win-core-psapi-l1-1-0.dll",
            "api-ms-win-core-psapi-ansi-l1-1-0.dll",
            "api-ms-win-core-profile-l1-1-0.dll",
            "API-MS-Win-Core-ProcessTopology-Obsolete-L1-1-0.dll",
            "api-ms-win-core-processthreads-l1-1-2.dll",
            "api-ms-win-core-processthreads-l1-1-1.dll",
            "api-ms-win-core-processthreads-l1-1-0.dll",
            "api-ms-win-core-processsecurity-l1-1-0.dll",
            "api-ms-win-core-processenvironment-l1-2-0.dll",
            "api-ms-win-core-processenvironment-l1-1-0.dll",
            "api-ms-win-core-privateprofile-l1-1-1.dll",
            "API-MS-Win-Core-PrivateProfile-L1-1-0.dll",
            "api-ms-win-core-normalization-l1-1-0.dll",
            "api-ms-win-core-namedpipe-l1-2-1.dll",
            "api-ms-win-core-namedpipe-l1-1-0.dll",
            "api-ms-win-core-memory-l1-1-3.dll",
            "api-ms-win-core-memory-l1-1-2.dll",
            "api-ms-win-core-memory-l1-1-1.dll",
            "api-ms-win-core-memory-l1-1-0.dll",
            "api-ms-win-core-localization-obsolete-l1-2-0.dll",
            "api-ms-win-core-localization-l2-1-0.dll",
            "api-ms-win-core-localization-l1-2-1.dll",
            "api-ms-win-core-localization-l1-2-0.dll",
            "api-ms-win-core-libraryloader-l1-1-1.dll",
            "api-ms-win-core-libraryloader-l1-1-0.dll",
            "API-MS-Win-Core-Kernel32-Private-L1-1-2.dll",
            "API-MS-Win-Core-Kernel32-Private-L1-1-1.dll",
            "API-MS-Win-Core-Kernel32-Private-L1-1-0.dll",
            "api-ms-win-core-kernel32-legacy-l1-1-2.dll",
            "api-ms-win-core-kernel32-legacy-l1-1-1.dll",
            "api-ms-win-core-kernel32-legacy-l1-1-0.dll",
            "api-ms-win-core-io-l1-1-1.dll",
            "api-ms-win-core-io-l1-1-0.dll",
            "api-ms-win-core-interlocked-l1-1-0.dll",
            "api-ms-win-core-heap-obsolete-l1-1-0.dll",
            "api-ms-win-core-heap-l1-1-0.dll",
            "api-ms-win-core-handle-l1-1-0.dll",
            "api-ms-win-core-file-l2-1-1.dll",
            "api-ms-win-core-file-l2-1-0.dll",
            "api-ms-win-core-file-l1-2-1.dll",
            "api-ms-win-core-file-l1-2-0.dll",
            "api-ms-win-core-file-l1-1-0.dll",
            "api-ms-win-core-fibers-l1-1-1.dll",
            "api-ms-win-core-fibers-l1-1-0.dll",
            "api-ms-win-core-errorhandling-l1-1-1.dll",
            "api-ms-win-core-errorhandling-l1-1-0.dll",
            "api-ms-win-core-delayload-l1-1-0.dll",
            "api-ms-win-core-debug-l1-1-1.dll",
            "api-ms-win-core-debug-l1-1-0.dll",
            "api-ms-win-core-datetime-l1-1-1.dll",
            "api-ms-win-core-datetime-l1-1-0.dll",
            "api-ms-win-core-console-l2-1-0.dll",
            "api-ms-win-core-console-l1-1-0.dll",
            "api-ms-win-core-comm-l1-1-0.dll",
            "api-ms-win-core-com-private-l1-1-0.dll",
            "api-ms-win-core-com-l1-1-0.dll",
            "API-MS-Win-Base-Util-L1-1-0.dll"
        };

        public void Setup(object value)
        {
            var ExampleValue = value as IExample;
            if (ExampleValue != null)
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
	
You will notice a couple of things with this code. The first is the most annoying part which is the assembly reference. Thanks to .Net Core's approach to assembly referencing it pushes everything into a dotnet folder. On Linux, Mac, etc. it will be in a different location. On top of that there are a number of DLLs that do not have metadata associated with them and Roslyn breaks if you try to compile against them. As such, if you're using this, you will probably prefer doing a more specified load of just the DLLs that you need and not everything in the NetCore.App directory. Similarly you will need to make sure you are pointing to the correct version. The above is just pointing to 1.0.0, change it accordingly if you wish to target 1.1 or 1.0.1 or something else.

The next bit of code to notice is that you must define any special interfaces you wish the newly generated type to inherit from. It is not possible to specify a base class as the type that is being generated will already be declared based on the type requested. Third property to notice is the Usings property. In the case above we aren't using any but if you would like to add any, that is where you do it. 

There are then a couple of functions in this class. SetupDefaultConstructor allows you to inject code into the default constructor for the type that you are generating. Any class setup code should be handled here. SetupInterfaces is where you would declare any functions, properties, or fields needed in order to implement the interfaces that you specified earlier. In the example above it returns the MySecretData property defined in the IExample interface. The SetupStartMethod is where you would return any code that you wish to inject at the beginning of the method. SetupEndMethod similarly is where you return any code that you wish to inject at the end of a method call. And lastly there is SetupExceptionMethod. This is where you would return any code that you wish to inject when an exception is thrown by the method.

After the code is constructed, the user calls the Create function on the Aspectus class:

    public class AOPTestClass
    {
        public virtual string A { get; set; }

        public virtual int B { get; set; }

        public virtual float C { get; set; }

        public virtual List<string> D { get; set; }
    }

	...
	
    var Test = Canister.Builder.Bootstrapper.Resolve<Aspectus>();
    var Item = (AOPTestClass)Test.Create(typeof(AOPTestClass));
	
This will construct a new instance of AOPTestClass and return it to the user. Before it returns it, it will call into your aspect one last time calling the Setup method, passing in the object created. When it does that you can modify the object however you would like prior to it being returned to the end user. From there the end user can modify, use, and debug their object without any issue.

## AOP Modules

While you can just create an Aspectus object and call Setup to implement items, you can actually do this when the the object is created for the first time by implementing the IAOPModule interface. By doing this you can tell the system to find and load your setup code all at once instead of doing it in chunks.

## Installation

The library is available via Nuget with the package name "Aspectus". To install it run the following command in the Package Manager Console:

Install-Package Aspectus

## Build Process

In order to build the library you will require the following:

1. Visual Studio 2015 with Update 3
2. .Net Core 1.0 SDK

Other than that, just clone the project and you should be able to load the solution and build without too much effort.


