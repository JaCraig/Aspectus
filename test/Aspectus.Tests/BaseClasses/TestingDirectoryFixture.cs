using Aspectus.ExtensionMethods;
using FileCurator;
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
            if (Canister.Builder.Bootstrapper == null)
            {
                var services = new ServiceCollection();
                services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
                Canister.Builder.CreateContainer(services,
                   typeof(TestingDirectoryFixture).GetTypeInfo().Assembly,
                   typeof(FileInfo).GetTypeInfo().Assembly)
                   .RegisterAspectus()
                   .Build();
            }

            new DirectoryInfo(@".\Testing").Create();
            new DirectoryInfo(@".\App_Data").Create();
            new DirectoryInfo(@".\Logs").Create();
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        public static ILogger Logger => Canister.Builder.Bootstrapper.Resolve<ILogger>();

        /// <summary>
        /// Gets the object pool.
        /// </summary>
        /// <value>The object pool.</value>
        public static ObjectPool<StringBuilder> ObjectPool => Canister.Builder.Bootstrapper.Resolve<ObjectPool<StringBuilder>>();

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            new DirectoryInfo(@".\Testing").Delete();
            new DirectoryInfo(@".\App_Data").Delete();
            new DirectoryInfo(@".\Logs").Delete();
            GC.SuppressFinalize(this);
        }
    }
}