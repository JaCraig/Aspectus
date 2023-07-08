using Aspectus.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Aspectus.Example;

/// <summary>
/// Example interface that will be implemented by the aspect
/// </summary>
public interface IExample
{
    /// <summary>
    /// Example property that will be set by the aspect
    /// </summary>
    /// <value>Example property that will be set by the aspect</value>
    string MySecretData { get; set; }
}

/// <summary>
/// Example class that will be modified by the aspect
/// </summary>
public class AOPTestClass
{
    /// <summary>
    /// Example property. In order to override it, the property must be virtual. Note that the
    /// system will skip non virtual properties.
    /// </summary>
    public virtual string? A { get; set; }

    /// <summary>
    /// Example property. In order to override it, the property must be virtual. Note that the
    /// system will skip non virtual properties.
    /// </summary>
    public virtual int B { get; set; }

    /// <summary>
    /// Example property. In order to override it, the property must be virtual. Note that the
    /// system will skip non virtual properties.
    /// </summary>
    public virtual float C { get; set; }

    /// <summary>
    /// Example property. In order to override it, the property must be virtual. Note that the
    /// system will skip non virtual properties.
    /// </summary>
    public virtual List<string> D { get; set; } = new List<string>();
}

/// <summary>
/// Example application showing a basic example of how to use Aspectus
/// </summary>
internal class Program
{
    /// <summary>
    /// Defines the entry point of the application.
    /// </summary>
    /// <param name="args">The arguments.</param>
    private static void Main(string[] args)
    {
        // Setup the service provider
        ServiceProvider? ServiceProvider = new ServiceCollection().AddCanisterModules()?.BuildServiceProvider();
        if (ServiceProvider is null)
            return;
        Aspectus Aspectus = ServiceProvider.GetRequiredService<Aspectus>();

        // Setup the aspectus system with the AOPTestClass type
        Aspectus.Setup(typeof(AOPTestClass));

        // Create an instance of the AOPTestClass
        AOPTestClass Object = Aspectus.Create<AOPTestClass>();

        // All the properties are the default values and work as normal.
        Console.WriteLine(Object.A);
        Console.WriteLine(Object.B);
        Console.WriteLine(Object.C);

        // Adding to the list works as normal.
        Object.D.Add("Test");
        Console.WriteLine(Object.D.First());

        // However our aspect has added a new property and interface to the class and we can access it.
        Console.WriteLine((Object as IExample)?.MySecretData);
    }
}

/// <summary>
/// Our aspect that will implement the interface
/// </summary>
/// <seealso cref="Aspectus.Interfaces.IAspect"/>
public class TestAspect : IAspect
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestAspect"/> class.
    /// </summary>
    public TestAspect()
    {
        // This is where we load the assemblies that we want to use.
        AssembliesUsing = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(TestAspect).GetTypeInfo().Assembly.Location)
            };
        foreach (FileInfo? DLL in new FileInfo(typeof(object).GetTypeInfo().Assembly.Location).Directory
                                                    .EnumerateFiles("*.dll")
                                                    .Where(x => Load.Contains(x.Name)))
        {
            PortableExecutableReference TempAssembly = MetadataReference.CreateFromFile(DLL.FullName);
            AssembliesUsing.Add(TempAssembly);
        }
    }

    /// <summary>
    /// Set of assemblies that the aspect requires
    /// </summary>
    public ICollection<MetadataReference> AssembliesUsing { get; }

    /// <summary>
    /// List of interfaces that need to be injected by this aspect
    /// </summary>
    public ICollection<Type> InterfacesUsing { get; } = new Type[] { typeof(IExample) };

    /// <summary>
    /// Using statements that the aspect requires
    /// </summary>
    public ICollection<string> Usings { get; } = Array.Empty<string>();

    /// <summary>
    /// The DLLs we want to load and use.
    /// </summary>
    private readonly string[] Load =
        {
            "mscorlib.dll",
"mscorlib.ni.dll",
"System.Collections.Concurrent.dll",
"System.Collections.dll",
"System.Collections.Immutable.dll",
"System.Runtime.dll"
        };

    /// <summary>
    /// Used to hook into the object once it has been created
    /// </summary>
    /// <param name="value">Object created by the system</param>
    public void Setup(object value)
    {
        // We just want to set the property on the object to "BLAH"
        if (value is IExample ExampleValue)
            ExampleValue.MySecretData = "BLAH";
    }

    /// <summary>
    /// Used to insert code into the default constructor
    /// </summary>
    /// <param name="baseType">Base type</param>
    /// <returns>The code to insert</returns>
    public string SetupDefaultConstructor(Type baseType) => "";

    /// <summary>
    /// Used to insert code at the end of the method
    /// </summary>
    /// <param name="method">Overridding Method</param>
    /// <param name="baseType">Base type</param>
    /// <param name="returnValueName">Local holder for the value returned by the function</param>
    /// <returns>The code to insert</returns>
    public string SetupEndMethod(MethodInfo method, Type baseType, string returnValueName) => "";

    /// <summary>
    /// Used to insert code within the catch portion of the try/catch portion of the method
    /// </summary>
    /// <param name="method">Overridding Method</param>
    /// <param name="baseType">Base type</param>
    /// <returns>The code to insert</returns>
    public string SetupExceptionMethod(MethodInfo method, Type baseType) => "";

    /// <summary>
    /// Used to set up any interfaces, extra fields, methods, etc. prior to overridding any methods.
    /// </summary>
    /// <param name="type">Type of the object</param>
    /// <returns>The code to insert</returns>
    public string SetupInterfaces(Type type) => "public string MySecretData{get; set;}";

    /// <summary>
    /// Used to insert code at the beginning of the method
    /// </summary>
    /// <param name="method">Overridding Method</param>
    /// <param name="baseType">Base type</param>
    /// <returns>The code to insert</returns>
    public string SetupStartMethod(MethodInfo method, Type baseType) => "";
}