using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace InventoryManagementSystemUnitTest.IntegrationTests
{
    public class AuthRedirectTest : IClassFixture<TestApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthRedirectTest(TestApplicationFactory factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Theory]
        [InlineData("/Home/ManagerDashboard")]
        [InlineData("/Home/WarehouseDashboard")]
        [InlineData("/Home/SupplierDashboard")]
        public async Task ProtectedPages_ShouldRedirectToLogin(string url)
        {
            var response = await _client.GetAsync(url);

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Account/Login", response.Headers.Location!.ToString());
        }
    }
}
