using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class XfastController : ControllerBase
    {
        private readonly ILogger<XfastController> _logger;
        private readonly HttpClient _httpClient;
        private readonly DomainSettings _domainSettings;

        public XfastController(ILogger<XfastController> logger, HttpClient httpClient, IOptions<DomainSettings> domainSettings)
        {
            _logger = logger;
            _httpClient = httpClient;
            _domainSettings = domainSettings.Value;
        }

        [HttpPost(Name = "GetLinkXfast")]
        public async Task<ResultEntity> Get(RequestEntity requestEntity)
        {
            var result = new ResultEntity();
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(requestEntity.mv_id.ToString()), "mv_id");
            formData.Add(new StringContent(requestEntity.action.ToString()), "action");
            formData.Add(new StringContent(requestEntity.cache.ToString()), "cache");
            try
            {
                var response = await _httpClient.PostAsync(_domainSettings.Clipphot, formData);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ClippResponse>(responseContent);
                    if (apiResponse != null)
                    {
                        result.type = apiResponse.type;
                        result.url = apiResponse.data;
                        var web = new HtmlWeb();
                        var doc = web.Load(result.url);
                        var streamNode = doc.DocumentNode.SelectSingleNode("//script[contains(text(), '.m3u8')]");

                        if (streamNode != null)
                        {
                            var scriptContent = streamNode.InnerText;
                            var streamUrl = ExtractM3u8Url(scriptContent);

                            if (!string.IsNullOrEmpty(streamUrl))
                            {
                                //var a = streamUrl;
                                result.m3u8_link = "https://xfast.sbs" + streamUrl;
                                result.guid = "e125cab8355";
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogError("API call failed with status code: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while calling the API.");
            }

            return result;
        }

        private string ExtractM3u8Url(string scriptContent)
        {
            // Sử dụng regex để tìm URL m3u8
            var regex = new Regex(@"(/media/[\w/.-]+\.m3u8)");
            var match = regex.Match(scriptContent);

            if (match.Success)
            {
                return match.Value; // Trả về đường dẫn m3u8
            }

            return null; // Không tìm thấy
        }
    }
}
