
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

        public string GenerateEmployeeCode(string name, string id)
        {
            
            var initials = string.Join("", name.Split(' ').Select(word => word[1])).ToUpper();

      
            var randomString = GenerateRandomString(4);  // Example: 6 characters for random string

         
            var suffix = "4M2024";

          
            var employeeCode = $"{initials}-{randomString}-{suffix}";

            return employeeCode;
        }

        
        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
