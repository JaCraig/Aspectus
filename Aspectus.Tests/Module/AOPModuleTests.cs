using Aspectus.Module;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Aspectus.Tests.Module
{
    public class AOPModuleTests
    {
        public AOPModuleTests()
        {
            _TestClass = new AOPModule();
        }

        private readonly AOPModule _TestClass;

        [Fact]
        public void CanCallLoad()
        {
            // Arrange
            IServiceCollection Bootstrapper = Substitute.For<IServiceCollection>();

            // Act
            _TestClass.Load(Bootstrapper);
        }

        [Fact]
        public void CanGetOrder()
        {
            // Assert
            var Result = Assert.IsType<int>(_TestClass.Order);

            Assert.Equal(2, Result);
        }
    }
}