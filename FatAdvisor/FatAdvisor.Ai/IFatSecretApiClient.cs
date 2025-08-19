using FatAdvisor.Ai.Models;

namespace FatAdvisor.Ai
{
    public interface IFatSecretApiClient
    {
        Task<string> GetMostEatenFoodsAsync();

        Task<string> GetTodayFoodsAsync();

        void SetAccessToken(TokenInfo tokenInfo);
    }
}
