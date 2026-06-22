using NUnit.Framework;
using Rotterdam.DigitalTwins.Runtime;
using Rotterdam.DigitalTwins.Editor.Utilities;
using System.Collections.Generic;

namespace Rotterdam.DigitalTwins.Editor.Tests
{
    [TestFixture]
    public class OUPFilterTests
    {
        [Test]
        public void FilterDatasets_WithSearchTerm_ReturnsMatchingDatasets()
        {
            // Arrange
            var datasets = new List<OUPDataset>
            {
                new OUPDataset { title = "Rotterdam Buildings", description = "3D models of buildings" },
                new OUPDataset { title = "Amsterdam Parks", description = "Green areas" },
                new OUPDataset { title = "Utrecht Water", description = "Canals and rivers" }
            };
            string searchTerm = "Rotterdam";

            // Act
            var result = OUPFilterUtility.FilterDatasets(datasets, searchTerm, null, null);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].title, Is.EqualTo("Rotterdam Buildings"));
        }

        [Test]
        public void FilterDatasets_WithHubId_ReturnsMatchingDatasets()
        {
            // Arrange
            var hub1 = new OUPHub { _id = "hub-123" };
            var hub2 = new OUPHub { _id = "hub-456" };
            var datasets = new List<OUPDataset>
            {
                new OUPDataset { title = "D1", ownerHub = hub1 },
                new OUPDataset { title = "D2", ownerHub = hub2 },
                new OUPDataset { title = "D3", ownerHub = hub1 }
            };
            string hubId = "hub-123";

            // Act
            var result = OUPFilterUtility.FilterDatasets(datasets, null, hubId, null);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            foreach (var d in result)
            {
                Assert.That(d.ownerHub._id, Is.EqualTo(hubId));
            }
        }

        [Test]
        public void FilterDatasets_WithFormats_ReturnsMatchingDatasets()
        {
            // Arrange
            var datasets = new List<OUPDataset>
            {
                new OUPDataset 
                { 
                    title = "D1", 
                    resources = new List<OUPResource> { new OUPResource { format = "I3S" } } 
                },
                new OUPDataset 
                { 
                    title = "D2", 
                    resources = new List<OUPResource> { new OUPResource { format = "WMS" } } 
                }
            };
            var formats = new List<string> { "I3S" };

            // Act
            var result = OUPFilterUtility.FilterDatasets(datasets, null, null, formats);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].title, Is.EqualTo("D1"));
        }
    }
}
