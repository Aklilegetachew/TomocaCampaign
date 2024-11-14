using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace TomocaCampaignAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BotWebhook : ControllerBase
    {
        private readonly ILogger<BotWebhook> _logger;

        public BotWebhook(ILogger<BotWebhook> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> HandleUpdate([FromBody] JObject update)
        {
            _logger.LogInformation("Received update: {Update}", update);
            

            // Check if the update contains new chat members
            var newMembers = update["message"]?["new_chat_members"];
            if (newMembers != null)
            {
                foreach (var member in newMembers)
                {
                    var username = member["username"]?.ToString() ?? member["first_name"]?.ToString();
                    var userId = member["id"]?.ToString();
                    _logger.LogInformation("New member joined: {Username} (ID: {UserId})", username, userId);

                
                }
            }

           
            return Ok();
        }

    }
}
