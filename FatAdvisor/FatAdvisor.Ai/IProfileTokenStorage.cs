using FatAdvisor.Ai.Models;

namespace FatAdvisor.Ai
{
    public interface IProfileTokenStorage
    {
        Task<TokenInfo> FindTokenInStorage();

        Task SaveTokenToStorage(TokenInfo accessTokenInfo);
    }
}
