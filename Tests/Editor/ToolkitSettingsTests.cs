using NUnit.Framework;
using DefaultNamespace;

namespace Tests.Editor
{
    public class ToolkitSettingsTests
    {
        [Test]
        public void FormatMessage_WithLoggingEnabled_ReturnsPrefixedMessage()
        {
            var settings = new ToolkitSettings { EnableLogging = true, LogPrefix = "[Test]" };
            var result = settings.FormatMessage("Hello");
            Assert.AreEqual("[Test] Hello", result);
        }

        [Test]
        public void FormatMessage_WithLoggingDisabled_ReturnsOriginalMessage()
        {
            var settings = new ToolkitSettings { EnableLogging = false };
            var result = settings.FormatMessage("Hello");
            Assert.AreEqual("Hello", result);
        }
    }
}
