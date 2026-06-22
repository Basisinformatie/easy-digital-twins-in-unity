using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using Rotterdam.DigitalTwins.Runtime;

namespace Rotterdam.DigitalTwins.Editor.Tests
{
    [TestFixture]
    public class OUPApiIntegrationTests
    {
        private HttpClient _httpClient;

        [SetUp]
        public void Setup()
        {
            _httpClient = new HttpClient();
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient?.Dispose();
        }

        [Test]
        [Category("Integration")]
        public async Task OUPApi_DatasetsEndpoint_IsReachable()
        {
            // Arrange
            string url = "https://hub.clearly.app/api/datasets?limit=1";

            // Act
            var response = await _httpClient.GetAsync(url);

            // Assert
            Assert.That(response.IsSuccessStatusCode, Is.True, $"API request to {url} failed with status {response.StatusCode}");
            
            string content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Is.Not.Null.And.Not.Empty);
            
            // Verify it can be parsed (basic check)
            // Note: OUP API returns an array or an object depending on version, 
            // OUPCatalogService handles both.
            using (JsonDocument doc = JsonDocument.Parse(content))
            {
                Assert.That(doc.RootElement.ValueKind, Is.AnyOf(JsonValueKind.Array, JsonValueKind.Object));
            }
        }
        
        [Test]
        [Category("Integration")]
        public async Task OUPApi_HubsEndpoint_ReturnsValidData()
        {
            // Arrange
            string url = "https://hub.clearly.app/api/hubs";

            // Act
            var response = await _httpClient.GetAsync(url);
            
            // Assert
            Assert.That(response.IsSuccessStatusCode, Is.True);
            string content = await response.Content.ReadAsStringAsync();
            
            // Use our models to see if they are still compatible
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                IncludeFields = true
            };
            
            var responseData = JsonSerializer.Deserialize<OUPHubResponse>(content, options);
            Assert.That(responseData, Is.Not.Null);
            Assert.That(responseData.results, Is.Not.Null);
            
            if (responseData.results.Count > 0)
            {
                Assert.That(responseData.results[0]._id, Is.Not.Null.And.Not.Empty);
            }
        }
    }
}
