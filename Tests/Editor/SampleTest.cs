using NUnit.Framework;

namespace Tests.Editor
{
    public class SampleTest
    {
        [Test]
        public void AlwaysPassingTest()
        {
            Assert.Pass("+++++++++++++Deze test slaagt altijd.++++++++++++++++");
        }
        
        public void PassingTest()
        {
            Assert.Pass("+++++++++++++Deze test slaagt ook.++++++++++++++++");
        }
    }
}
