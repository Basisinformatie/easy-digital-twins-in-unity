using NUnit.Framework;
using Rotterdam.DigitalTwins.Editor.Utilities;

namespace Rotterdam.DigitalTwins.Editor.Tests
{
    [TestFixture]
    public class ProjectTypeDetectorTests
    {
        [Test]
        public void GetProjectTypes_WithSingleType_ReturnsCorrectString()
        {
            var type = ProjectType.VR;
            string result = ProjectTypeStrings.GetProjectTypes(type);
            Assert.That(result, Is.EqualTo("VR"));
        }

        [Test]
        public void GetProjectTypes_WithMultipleTypes_ReturnsCommaSeparatedString()
        {
            var type = ProjectType.VR | ProjectType.AR;
            string result = ProjectTypeStrings.GetProjectTypes(type);
            Assert.That(result, Is.EqualTo("VR, AR"));
        }

        [Test]
        public void GetProjectTypes_WithNone_ReturnsUnknown()
        {
            var type = ProjectType.None;
            string result = ProjectTypeStrings.GetProjectTypes(type);
            Assert.That(result, Is.EqualTo("Unknown"));
        }

        [Test]
        public void GetCompatibility_WithMultiplePlatforms_ReturnsCorrectString()
        {
            var type = ProjectType.Android | ProjectType.Windows | ProjectType.Mac;
            string result = ProjectTypeStrings.GetCompatibility(type);
            Assert.That(result, Is.EqualTo("Android, Windows, MAC"));
        }

        [Test]
        public void GetCompatibility_WithNone_ReturnsNone()
        {
            var type = ProjectType.None;
            string result = ProjectTypeStrings.GetCompatibility(type);
            Assert.That(result, Is.EqualTo("None"));
        }

        [Test]
        public void GetCompatibility_WithWebApp_ReturnsWebApp()
        {
            var type = ProjectType.WebApp;
            string result = ProjectTypeStrings.GetCompatibility(type);
            Assert.That(result, Is.EqualTo("Web app"));
        }
    }
}
