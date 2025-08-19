namespace FatAdvisor.Ai.Models
{
    public class TokenInfo
    {
        public TokenInfo(string accessToken, string accessTokenSecret)
        {
            Token = accessToken;
            TokenSecret = accessTokenSecret;
        }

        public string Token { get; set; }

        public string TokenSecret { get; set; }
    }
}
