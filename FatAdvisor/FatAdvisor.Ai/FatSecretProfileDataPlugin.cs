using FatAdvisor.Ai.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace FatAdvisor.Ai
{
    public class FatSecretProfileDataPlugin
    {
        private readonly IFatSecretApiClient _fatSecretApiClient;
        private readonly IFatSecretOAuth _fatSecretOAuth;
        private readonly IProfileTokenStorage _profileTokenStorage;

        public FatSecretProfileDataPlugin(
            IFatSecretApiClient fatSecretApiClient,
            IFatSecretOAuth fatSecretOAuth,
            IProfileTokenStorage profileTokenStorage)
        {
            _fatSecretApiClient = fatSecretApiClient;
            _fatSecretOAuth = fatSecretOAuth;
            _profileTokenStorage = profileTokenStorage;
        }

        [KernelFunction("get_todays_food")]
        [Description("Get all the eaten food for today")]
        public async Task<string> GetTodaysFood()
        {
            var accessTokenInfo = await _profileTokenStorage.FindTokenInStorage();
            if (accessTokenInfo == null)
            {
                // If no token is found, we need to authorize the user
                accessTokenInfo = await AuthorizeAndSaveToken();
            }

            _fatSecretApiClient.SetAccessToken(accessTokenInfo);

            var todaysFood = await _fatSecretApiClient.GetTodayFoodsAsync();
            return todaysFood;
        }

        private async Task<TokenInfo> AuthorizeAndSaveToken()
        {
            string callbackUrl = null; //For prototype we could use null.

            var requestTokenInfo = await _fatSecretOAuth.GetRequestTokenAsync(callbackUrl);
            var authorizationUrl = _fatSecretOAuth.GetAuthorizationUrl(
                requestTokenInfo.Token,
                callbackUrl);

            Console.WriteLine($"Please visit: {authorizationUrl}");

            Console.WriteLine("\nPaste the verifier code here:");
            string verifier = Console.ReadLine() ?? "";

            var accessTokenInfo = await _fatSecretOAuth.GetAccessTokenAsync(
                requestTokenInfo.Token,
                requestTokenInfo.TokenSecret,
                verifier);

            await _profileTokenStorage.SaveTokenToStorage(accessTokenInfo);

            return accessTokenInfo;
        }
    }
}
