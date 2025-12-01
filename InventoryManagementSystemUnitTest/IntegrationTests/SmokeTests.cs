using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace InventoryManagementSystemUnitTest.IntegrationTests
{
    public class SmokeTests : IClassFixture<TestApplicationFactory>
    {
        private readonly HttpClient _client;

        public SmokeTests(TestApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task HomePage_Returns200()
        {
            var response = await _client.GetAsync("/");
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task LoginRoute_Returns200()
        {
            var response = await _client.GetAsync("/Account/Login");
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task InvalidRoute_Returns404()
        {
            var response = await _client.GetAsync("/THIS_DOES_NOT_EXIST");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
