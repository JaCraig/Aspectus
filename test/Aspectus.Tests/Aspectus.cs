using Aspectus.CodeGen;
using Aspectus.Interfaces;
using Aspectus.Tests.BaseClasses;
using System.Collections.Generic;
using Xunit;

namespace Aspectus.Tests
{
    public class AOPTestClass
    {
        public virtual string A { get; set; }

        public virtual int B { get; set; }

        public virtual float C { get; set; }

        public virtual List<string> D { get; set; }
    }

    public class AspectusTests : TestingDirectoryFixture
    {
        [Fact]
        public void Create()
        {
            var Test = new Aspectus(new Compiler(), new List<IAspect>(), new List<IAOPModule>());
            var Item = (AOPTestClass)Test.Create(typeof(AOPTestClass));
            Assert.NotNull(Item);
        }
    }
}