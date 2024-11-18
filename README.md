# Aspectus

[![.NET Publish](https://github.com/JaCraig/Aspectus/actions/workflows/dotnet-publish.yml/badge.svg)](https://github.com/JaCraig/Aspectus/actions/workflows/dotnet-publish.yml) [![Coverage Status](https://coveralls.io/repos/github/JaCraig/Aspectus/badge.svg?branch=master)](https://coveralls.io/github/JaCraig/Aspectus?branch=master)

Aspectus is an advanced Aspect-Oriented Programming (AOP) library that simplifies the injection of cross-cutting concerns into your codebase. It empowers you to write clean and maintainable code by separating cross-cutting concerns from the core logic of your application.

## Key Features

- **Easy Integration**: Aspectus seamlessly integrates with your project by registering with the IoC (Inversion of Control) container during startup.
- **Code Generation**: Leveraging Roslyn, Aspectus generates code dynamically, allowing you to write expressive C# code for implementing aspects.
- **Flexible Aspect Customization**: Implement the `IAspect` interface to define custom logic for constructors, methods, and exception handling.
- **AOP Modules**: Aspectus supports modules, enabling you to consolidate and load setup code efficiently.
- **NuGet Package**: Install Aspectus easily through NuGet, simplifying the setup process for your projects.

## Installation

To install Aspectus, use the NuGet package manager:

```
Install-Package Aspectus
```

## Getting Started

Follow these steps to start using Aspectus in your project:

1. Register Aspectus with your IoC container during startup. Example code for ASP.NET Core:

   ```csharp
   ServiceProvider? ServiceProvider = new ServiceCollection().RegisterObjectCartographer()?.BuildServiceProvider();
   ```

   or

   ```csharp
   ServiceProvider? ServiceProvider = new ServiceCollection().AddCanisterModules()?.BuildServiceProvider();
   ```

   As the library supports [Canister Modules](https://github.com/JaCraig/Canister).

3. Implement aspects by creating classes that inherit from the `IAspect` interface. Customize aspects based on your specific requirements, such as constructor setups, method injections, and exception handling.

   ```csharp
   public class TestAspect : IAspect
   {
       // Implement your aspect logic here
   }
   ```

4. Utilize Aspectus to create instances of types with injected aspects.

   ```csharp
   var aspectus = ServiceProvider.GetRequiredService<Aspectus>();
   aspectus.Setup(typeof(YourClass));
   var item = aspectus.Create<YourClass>();
   // Use and enjoy your enhanced object
   ```

For a more detailed guide on using Aspectus, including advanced scenarios and AOP modules, refer to the [Aspectus Documentation](https://jacraig.github.io/Aspectus/articles/intro.html).

## Build Process

To build Aspectus from source, ensure you have the following:

- Visual Studio 2022
- .Net 8

Simply clone the repository and open the solution in Visual Studio.

## Contributing

Contributions are welcome! To contribute to Aspectus, please follow these steps:

1. Fork the repository.
2. Create your feature branch: `git checkout -b feature/YourFeature`.
3. Commit your changes: `git commit -am 'Add YourFeature'`.
4. Push to the branch: `git push origin feature/YourFeature`.
5. Submit a pull request.

Please ensure your code follows the existing coding style and includes appropriate tests. Additionally, make sure to update the documentation as needed.
