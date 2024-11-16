using Aspectus.ExtensionMethods;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Aspectus.Tests.ExtensionMethods
{
    public static class RegistrationTests
    {
        [Fact]
        public static void CanCallRegisterAspectusWithBootstrapper()
        {
            // Arrange
            var Services = new ServiceCollection();

            // Act
            _ = Services.AddCanisterModules(config => config.RegisterAspectus());
        }

        [Fact]
        public static void CanCallRegisterAspectusWithServices()
        {
            // Arrange
            IServiceCollection Services = Substitute.For<IServiceCollection>();

            // Act
            IServiceCollection Result = Services.RegisterAspectus();

            // Assert
            Assert.Same(Services, Result);
        }
    }
}