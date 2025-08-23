using FatAdvisor.Ai;
using FatAdvisor.Ai.Models;
using System.Security.Cryptography;
using System.Text;

namespace FatAdvisor.FatSecretApi
{
    public class FatSecretApiClient : IFatSecretApiClient
    {
        private readonly HttpClient _httpClient;

        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private string _accessToken;
        private string _accessTokenSecret;

        public FatSecretApiClient(
            HttpClient httpClient,
            string consumerKey,
            string consumerSecret,
            string accessToken,
            string accessTokenSecret)
        {
            _httpClient = httpClient;

            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _accessToken = accessToken;
            _accessTokenSecret = accessTokenSecret;
        }

        public async Task<string> GetMostEatenFoodsAsync()
        {
            string url = "https://platform.fatsecret.com/rest/food/most-eaten/v2";

            var parameters = new SortedDictionary<string, string>
            {
                { "oauth_consumer_key", _consumerKey },
                { "oauth_token", _accessToken },
                { "oauth_nonce", Guid.NewGuid().ToString("N") },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() },
                { "oauth_version", "1.0" },
                { "format", "json" }
            };

            string signature = SignRequest("GET", url, parameters, _consumerSecret, _accessTokenSecret);
            parameters.Add("oauth_signature", signature);

            string query = string.Join("&", parameters.Select(kvp =>
                $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

            string requestUrl = $"{url}?{query}";
            var response = await _httpClient.GetAsync(requestUrl);
            return await response.Content.ReadAsStringAsync();
        }

        private static string SignRequest(string method, string url, SortedDictionary<string, string> parameters,
            string consumerSecret, string tokenSecret)
        {
            string paramString = string.Join("&", parameters.Select(kvp =>
                $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

            string baseString = $"{method.ToUpper()}&{Uri.EscapeDataString(url)}&{Uri.EscapeDataString(paramString)}";
            string key = $"{Uri.EscapeDataString(consumerSecret)}&{Uri.EscapeDataString(tokenSecret ?? "")}";

            using var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(key));
            byte[] hash = hasher.ComputeHash(Encoding.ASCII.GetBytes(baseString));
            return Convert.ToBase64String(hash);
        }

        public async Task<string> GetTodayFoodsAsync()
        {
            string url = "https://platform.fatsecret.com/rest/food-entries/v2";

            var today = DateTime.UtcNow.Date; // midnight UTC today

            int daysSinceEpoch = (int)(today - DateTime.UnixEpoch).TotalDays;

            var parameters = new SortedDictionary<string, string>
            {
                { "oauth_consumer_key", _consumerKey },
                { "oauth_token", _accessToken },
                { "oauth_nonce", Guid.NewGuid().ToString("N") },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() },
                { "oauth_version", "1.0" },
                { "date", daysSinceEpoch.ToString() },
                { "format", "json" }
            };

            string signature = SignRequest("GET", url, parameters, _consumerSecret, _accessTokenSecret);
            parameters.Add("oauth_signature", signature);

            string query = string.Join("&", parameters.Select(kvp =>
                $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

            string requestUrl = $"{url}?{query}";
            var response = await _httpClient.GetAsync(requestUrl);
            return await response.Content.ReadAsStringAsync();
        }

        public void SetAccessToken(TokenInfo tokenInfo)
        {
            this._accessToken = tokenInfo.Token;
            this._accessTokenSecret = tokenInfo.TokenSecret;
        }
    }
}
