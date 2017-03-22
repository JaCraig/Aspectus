using Aspectus.Interfaces;
using FileCurator;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
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
            foreach (var DLL in new DirectoryInfo(@"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\1.0.4\")
                                                        .EnumerateFiles("*.dll")
                                                        .Where(x => !DontLoad.Contains(x.Name)))
            {
                var TempAssembly = MetadataReference.CreateFromFile(DLL.FullName);
                AssembliesUsing.Add(TempAssembly);
            }
        }

        private string[] DontLoad =
                                        {
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

        public ICollection<MetadataReference> AssembliesUsing { get; private set; }

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