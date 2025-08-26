using FatAdvisor.Ai.Models;

namespace FatAdvisor.Ai
{
    public interface IFatSecretApiClient
    {
        Task<string> GetMostEatenFoodsAsync();

        Task<string> GetFoodsForDateAsync(DateTime date);

        Task<string> GetWeightDiary(DateTime date);

        void SetAccessToken(TokenInfo tokenInfo);
    }
}
