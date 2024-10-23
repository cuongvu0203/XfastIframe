using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;

namespace Webframe.Pages
{
    public class PlayerModel : PageModel
    {
        private readonly ILogger<PlayerModel> _logger;
        private readonly HttpClient _httpClient;
        public PlayerModel(ILogger<PlayerModel> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public void OnGet()
        {

        }

        public async Task<IActionResult> GetM3U8(string url)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Referrer = new Uri("https://xfast.sbs/watch/ca185569647.html");

            var response = await _httpClient.SendAsync(requestMessage);
            var content = await response.Content.ReadAsByteArrayAsync();

            return File(content, response.Content.Headers.ContentType.ToString());
        }
    }
}
