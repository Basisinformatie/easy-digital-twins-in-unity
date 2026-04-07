using NUnit.Framework;
using DefaultNamespace;

namespace Tests.Editor
{
    public class OUPConfigTests
    {
        [Test]
        public void IsValid_DefaultConfig_ReturnsTrue()
        {
            var config = new OUPConfig();
            Assert.IsTrue(config.IsValid());
        }

        [Test]
        public void IsValid_EmptyApiUrl_ReturnsFalse()
        {
            var config = new OUPConfig { ApiUrl = "" };
            Assert.IsFalse(config.IsValid());
        }

        [Test]
        public void IsValid_ZeroTimeout_ReturnsFalse()
        {
            var config = new OUPConfig { TimeoutSeconds = 0 };
            Assert.IsFalse(config.IsValid());
        }
    }
}
