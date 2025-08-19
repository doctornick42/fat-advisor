using FatAdvisor.Ai.Models;

namespace FatAdvisor.Ai
{
    public interface IFatSecretOAuth
    {
        Task<TokenInfo> GetRequestTokenAsync(string callbackUrl);

        string GetAuthorizationUrl(string requestToken, string callbackUrl);

        Task<TokenInfo> GetAccessTokenAsync(
            string requestToken,
            string requestTokenSecret,
            string verifier);
    }
}
