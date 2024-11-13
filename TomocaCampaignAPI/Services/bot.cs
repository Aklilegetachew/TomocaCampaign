using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;

namespace TomocaCampaignAPI.Services
{
    public class Bot
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Bot> _logger;

        public Bot(HttpClient httpClient, IConfiguration configuration, ILogger<Bot> logger)
        {
            _logger = logger;
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri(_configuration["ThirdPartyApi:TelegramBaseUrl"]);
        }

        public async Task<string> CreateInviteString(string chatId, string name)
        {
            var requestBody = new
            {
                chat_id = chatId,
                name = name
            };

            // Logging the start of the process
            _logger.LogInformation("Processing bot for chatId: {ChatId} and name: {Name}", chatId, name);

            try
            {
                var response = await _httpClient.PostAsJsonAsync("createChatInviteLink", requestBody);

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as string
                    var responseBody = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Telegram API Response: {ResponseBody}", responseBody);

                    // Optionally, deserialize it to check for specific fields
                    var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseBody);

                    // Log the invite link if the response contains it
                    if (jsonResponse?.ok == true)
                    {
                        //_logger.LogInformation("Invite link generated: {InviteLink}", jsonResponse.result.invite_link);
                        return jsonResponse.result.invite_link;
                    }
                    else
                    {
                        //_logger.LogError("API response 'ok' is false. Error: {Error}", jsonResponse?.error_code);
                        throw new Exception("API response 'ok' is false.");
                    }
                }
                else
                {
                    _logger.LogError("Error response from Telegram API: StatusCode = {StatusCode}", response.StatusCode);
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error response body: {ErrorResponse}", errorResponse);
                    throw new Exception("Error creating an invitation link");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred: {ExceptionMessage}", ex.Message);
                throw;
            }
        }
    }
}
