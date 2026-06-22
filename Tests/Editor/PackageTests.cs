using NUnit.Framework;
using System.IO;
using System.Text.Json;

namespace Rotterdam.DigitalTwins.Editor.Tests
{
    /// <summary>
    /// Basic tests to verify the package integrity.
    /// </summary>
    public class PackageTests
    {
        [Test]
        public void PackageJson_ExistsAndIsValid()
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "package.json");

            Assert.That(File.Exists(path), Is.True, $"package.json not found at {path}");
            
            string content = File.ReadAllText(path);
            using (JsonDocument doc = JsonDocument.Parse(content))
            {
                Assert.That(doc.RootElement.GetProperty("name").GetString(), Is.EqualTo("com.rotterdam.digital-twins"));
            }
        }
    }
}
