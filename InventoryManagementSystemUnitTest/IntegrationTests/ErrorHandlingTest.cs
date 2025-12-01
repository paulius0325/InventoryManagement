using System.Net;
using Xunit;

namespace InventoryManagementSystemUnitTest.IntegrationTests
{
    public class ErrorHandlingTest : IClassFixture<TestApplicationFactory>
    {
        private readonly HttpClient _client;

        public ErrorHandlingTest(TestApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task RequestingUnknownPage_ShouldReturnCustomErrorPage()
        {
            var response = await _client.GetAsync("/__DOES_NOT_EXIST");

            Assert.True(response.StatusCode == HttpStatusCode.NotFound ||
                        response.StatusCode == HttpStatusCode.Redirect);
        }
    }
}
