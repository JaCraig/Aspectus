using Aspectus.ExtensionMethods;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Serilog;
using System;
using System.Reflection;
using System.Text;
using Xunit;

namespace Aspectus.Tests.BaseClasses
{
    /// <summary>
    /// Testing directory fixture
    /// </summary>
    /// <seealso cref="System.IDisposable"/>
    [Collection("DirectoryCollection")]
    public class TestingDirectoryFixture : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestingDirectoryFixture"/> class.
        /// </summary>
        public TestingDirectoryFixture()
        {
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        public static ILogger Logger => GetServiceProvider().GetService<ILogger>();

        /// <summary>
        /// Gets the object pool.
        /// </summary>
        /// <value>The object pool.</value>
        public static ObjectPool<StringBuilder> ObjectPool => GetServiceProvider().GetService<ObjectPool<StringBuilder>>();

        /// <summary>
        /// The service lock
        /// </summary>
        protected static object ServiceLock = new();

        /// <summary>
        /// The service provider
        /// </summary>
        protected static IServiceProvider ServiceProvider;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose() => GC.SuppressFinalize(this);

        /// <summary>
        /// Gets the service provider.
        /// </summary>
        /// <returns></returns>
        protected static IServiceProvider GetServiceProvider()
        {
            if (ServiceProvider is not null)
                return ServiceProvider;
            lock (ServiceLock)
            {
                if (ServiceProvider is not null)
                    return ServiceProvider;
                var services = new ServiceCollection();
                _ = services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
                _ = services.AddCanisterModules(configure => configure.AddAssembly(typeof(TestingDirectoryFixture).GetTypeInfo().Assembly)
                   .RegisterAspectus());
                ServiceProvider = services.BuildServiceProvider();
                return ServiceProvider;
            }
        }
    }
}