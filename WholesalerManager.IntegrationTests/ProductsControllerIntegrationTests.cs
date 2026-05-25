using FluentAssertions;

namespace WholesalerManager.IntegrationTests
{
    public class ProductsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ProductsControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Index_toReturnView()
        {
            HttpResponseMessage response = await _client.GetAsync("/Products/Index");

            response.IsSuccessStatusCode.Should().BeTrue();
        }
    }
}
