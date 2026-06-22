using NUnit.Framework;
using Rotterdam.DigitalTwins.Runtime;
using Rotterdam.DigitalTwins.Editor.Utilities;
using System.Collections.Generic;

namespace Rotterdam.DigitalTwins.Editor.Tests
{
    /// <summary>
    /// Unit tests for OUP filtering logic.
    /// </summary>
    [TestFixture]
    public class OUPFilterTests
    {
        [Test]
        public void FilterDatasets_WithSearchTerm_ReturnsMatchingDatasets()
        {
            var datasets = new List<OUPDataset>
            {
                new OUPDataset { title = "Utrecht Water", description = "Canals and rivers" },
                new OUPDataset { title = "Rotterdam Buildings", description = "3D models of buildings" },
                new OUPDataset { title = "Amsterdam Parks", description = "Green areas" },
            };
            string searchTerm = "Rotterdam";

            var result = OUPFilterUtility.FilterDatasets(datasets, searchTerm, null, null);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].title, Is.EqualTo("Rotterdam Buildings"));
        }

        [Test]
        public void FilterDatasets_WithHubId_ReturnsMatchingDatasets()
        {
            var hub1 = new OUPHub { _id = "hub-123" };
            var hub2 = new OUPHub { _id = "hub-456" };
            var datasets = new List<OUPDataset>
            {
                new OUPDataset { title = "D1", ownerHub = hub1 },
                new OUPDataset { title = "D2", ownerHub = hub2 },
                new OUPDataset { title = "D3", ownerHub = hub1 }
            };
            string hubId = "hub-123";

            var result = OUPFilterUtility.FilterDatasets(datasets, null, hubId, null);

            Assert.That(result, Has.Count.EqualTo(2));
            foreach (var d in result)
            {
                Assert.That(d.ownerHub._id, Is.EqualTo(hubId));
            }
        }

        [Test]
        public void FilterDatasets_WithFormats_ReturnsMatchingDatasets()
        {
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

            var result = OUPFilterUtility.FilterDatasets(datasets, null, null, formats);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].title, Is.EqualTo("D1"));
        }

        [Test]
        public void IsSearchTermValid_WithValidTerm_ReturnsTrue()
        {
            string searchTerm = "Rotterdam";

            bool isValid = OUPFilterUtility.IsSearchTermValid(searchTerm, out string errorMessage);

            Assert.That(isValid, Is.True);
            Assert.That(errorMessage, Is.Empty);
        }

        [Test]
        public void IsSearchTermValid_WithEmptyTerm_ReturnsTrue()
        {
            string searchTerm = "";

            bool isValid = OUPFilterUtility.IsSearchTermValid(searchTerm, out string errorMessage);

            Assert.That(isValid, Is.True);
            Assert.That(errorMessage, Is.Empty);
        }

        [Test]
        public void IsSearchTermValid_WithShortTerm_ReturnsTrue()
        {
            string searchTerm = "Ro";

            bool isValid = OUPFilterUtility.IsSearchTermValid(searchTerm, out string errorMessage);

            Assert.That(isValid, Is.True);
            Assert.That(errorMessage, Is.Empty);
        }

        [Test]
        public void IsSearchTermValid_WithInvalidCharacters_ReturnsFalse()
        {
            string searchTerm = "<script>";

            bool isValid = OUPFilterUtility.IsSearchTermValid(searchTerm, out string errorMessage);

            Assert.That(isValid, Is.False);
            Assert.That(errorMessage, Is.Not.Empty);
            Assert.That(errorMessage, Does.Contain("Only letters, numbers and spaces are allowed"));
        }

        [Test]
        public void IsSearchTermValid_WithOnlySpaces_ReturnsTrue()
        {
            string searchTerm = "   ";

            bool isValid = OUPFilterUtility.IsSearchTermValid(searchTerm, out string errorMessage);

            Assert.That(isValid, Is.True);
            Assert.That(errorMessage, Is.Empty);
        }

        [Test]
        public void IsSearchTermValid_WithTooLongTerm_ReturnsFalse()
        {
            string searchTerm = new string('a', 41);

            bool isValid = OUPFilterUtility.IsSearchTermValid(searchTerm, out string errorMessage);

            Assert.That(isValid, Is.False);
            Assert.That(errorMessage, Does.Contain("cannot exceed 40 characters"));
        }

        [Test]
        public void IsSearchTermValid_WithDigitsAndSpaces_ReturnsTrue()
        {
            string searchTerm = "Rotterdam 2024";

            bool isValid = OUPFilterUtility.IsSearchTermValid(searchTerm, out string errorMessage);

            Assert.That(isValid, Is.True);
        }

        [Test]
        public void IsSearchTermValid_WithExactlyThreeChars_ReturnsTrue()
        {
            string searchTerm = "abc";

            bool isValid = OUPFilterUtility.IsSearchTermValid(searchTerm, out string errorMessage);

            Assert.That(isValid, Is.True);
        }
    }
}
