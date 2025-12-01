using System.Net;
using Xunit;

namespace InventoryManagementSystemUnitTest.IntegrationTests
{
    public class CrudPageTest : IClassFixture<TestApplicationFactory>
    {
        private readonly HttpClient _client;

        public CrudPageTest(TestApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Theory]
        [InlineData("/Items")]
        [InlineData("/Items/Create")]
        [InlineData("/Orders")]
        [InlineData("/Users")]
        public async Task Pages_ShouldReturn_OK(string url)
        {
            var response = await _client.GetAsync(url);

            Assert.True(
                response.IsSuccessStatusCode ||
                response.StatusCode == HttpStatusCode.Redirect  // if protected
            );
        }
    }
}
