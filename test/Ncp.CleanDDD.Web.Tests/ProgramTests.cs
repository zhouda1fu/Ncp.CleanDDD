using System.Net.Http.Json;
using Ncp.CleanDDD.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ncp.CleanDDD.Web.Tests
{
    [Collection("web")]
    public class ProgramTests 
    {

        private readonly HttpClient _client;

        public ProgramTests(MyWebApplicationFactory factory)
        {
            _client = factory.WithWebHostBuilder(builder => { builder.ConfigureServices(_ => { }); })
             .CreateClient();
        }


        [Fact]
        public async Task HealthCheckTest()
        {
            var response = await _client.GetAsync("/health");
            Assert.True(response.IsSuccessStatusCode);
        }
    }
}