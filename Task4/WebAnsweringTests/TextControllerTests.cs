using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Xunit;
using WebAnsweringServer;
using WebAnsweringServer.Controllers;
using System.Text;
using System.Net.Http.Json;
using static System.Net.Mime.MediaTypeNames;
namespace WebAnsweringTests
{
    public class TextControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> factory;
        public TextControllerTests(WebApplicationFactory<Program> factory)
        {
            this.factory = factory;
        }
        [Fact]
        public async Task GetAnswerTest()
        {
            var client = factory.CreateClient();
            var postResponse =  await client.PostAsJsonAsync("https://localhost:7177/api/text", "Text1");
            postResponse.EnsureSuccessStatusCode();
            var textId = await postResponse.Content.ReadAsStringAsync();
            var getResponse = await client.GetAsync($"https://localhost:7177/api/text?textId={textId}&question=What?");
            getResponse.EnsureSuccessStatusCode();
            var answer = await getResponse.Content.ReadAsStringAsync();
            Assert.NotNull(answer );
        }
     
        [Fact]
        public async Task EmptyText()
        {
            var client = factory.CreateClient();
            var postResponse = await client.PostAsJsonAsync("https://localhost:7177/api/text", "");
            var textId = await postResponse.Content.ReadAsStringAsync();
            Assert.Equal(400, (int)postResponse.StatusCode);
            Assert.Equal("Empty text", textId);
        }
    }
}
