
namespace TomocaCampaignAPI.Services
{
    public class ReferralCodeService
    {
        private readonly Bot _botService;

        public ReferralCodeService(Bot botService)
        {
            _botService = botService;
        }

        public async Task<string> GenerateReferralCodeAsync(string name, string id)
        {
             var chatId = "@akgchannel";          
            var invitationLink = await _botService.CreateInviteString(chatId, name);

            return invitationLink;

        }
    }
}
