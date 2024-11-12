namespace TomocaCampaignAPI.Services
{
    public class ReferralCodeService
    {

        public string GenerateReferralCode(string name, string id)
        {
            // Use part of the name and a zero-padded ID for uniqueness
            var namePart = name.Length >= 3 ? name.Substring(0, 3).ToUpper() : name.ToUpper();
            var idPart = id; // Pads the ID with leading zeros, e.g., "0012"
            return $"{namePart}-{idPart}";
        }
    }
}
