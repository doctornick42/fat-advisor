using FatAdvisor.Ai.Models;

namespace FatAdvisor.Ai
{
    public class ProfileTokenStorage : IProfileTokenStorage
    {
        private readonly string _tokenFilePath = ".\\access_token.txt";

        public ProfileTokenStorage(string tokenFilePath = null)
        {
            if (!string.IsNullOrEmpty(tokenFilePath))
            {
                _tokenFilePath = tokenFilePath;
            }
        }

        public async Task<TokenInfo> FindTokenInStorage()
        {
            if (!File.Exists(_tokenFilePath))
            {
                return null;
            }

            var tokenFileContent = await File.ReadAllLinesAsync(_tokenFilePath);
            if (tokenFileContent.Length != 2)
            {
                Console.WriteLine("Access token file is corrupted or invalid.");
                return null;
            }

            return new TokenInfo(tokenFileContent[0], tokenFileContent[1]);
        }

        public Task SaveTokenToStorage(TokenInfo accessTokenInfo) =>
            File.WriteAllLinesAsync(
                _tokenFilePath,
                [accessTokenInfo.Token, accessTokenInfo.TokenSecret]);
    }
}
