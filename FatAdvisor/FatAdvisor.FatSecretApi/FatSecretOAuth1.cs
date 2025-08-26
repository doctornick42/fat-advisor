using FatAdvisor.Ai;
using FatAdvisor.Ai.Models;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace FatAdvisor.FatSecretApi
{
    public class FatSecretOAuth1 : IFatSecretOAuth
    {
        //TODO: handle auth exception and re-authorize user if needed

        private const string FatSecreyAuthorizeUrl = "https://authentication.fatsecret.com/oauth/authorize";
        private const string RequestTokenEndpointUrl = "https://authentication.fatsecret.com/oauth/request_token";
        private const string AccessTokenEndpointUrl = "https://authentication.fatsecret.com/oauth/access_token";
        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private readonly HttpClient _http;

        public FatSecretOAuth1(HttpClient http, string consumerKey, string consumerSecret)
        {
            _http = http;
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
        }

        public async Task<TokenInfo> GetRequestTokenAsync(string callbackUrl)
        {
            var parameters = new SortedDictionary<string, string>
            {
                ["oauth_consumer_key"] = _consumerKey,
                ["oauth_nonce"] = Guid.NewGuid().ToString("N"),
                ["oauth_signature_method"] = "HMAC-SHA1",
                ["oauth_timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ["oauth_version"] = "1.0",
                ["oauth_callback"] = string.IsNullOrEmpty(callbackUrl) ? "oob" : callbackUrl,
            };

            string signature = SignRequest("POST", RequestTokenEndpointUrl, parameters, _consumerSecret, null);
            parameters["oauth_signature"] = signature;

            var content = new FormUrlEncodedContent(parameters);
            var response = await _http.PostAsync(RequestTokenEndpointUrl, content);

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                //TODO: handle it better
                Console.WriteLine($"Error: {errorContent}");
                throw;
            }

            var body = await response.Content.ReadAsStringAsync();
            var parts = HttpUtility.ParseQueryString(body);

            return new TokenInfo(parts["oauth_token"], parts["oauth_token_secret"]);
        }

        public string GetAuthorizationUrl(string requestToken, string callbackUrl)
        {
            var uriBuilder = new UriBuilder(FatSecreyAuthorizeUrl)
            {
                Query = $"oauth_token={HttpUtility.UrlEncode(requestToken)}&oauth_callback={HttpUtility.UrlEncode(callbackUrl)}"
            };
            
            return uriBuilder.ToString();
        }

        public async Task<TokenInfo> GetAccessTokenAsync(
            string requestToken,
            string requestTokenSecret,
            string verifier)
        {
            var parameters = new SortedDictionary<string, string>
            {
                ["oauth_consumer_key"] = _consumerKey,
                ["oauth_token"] = requestToken,
                ["oauth_nonce"] = Guid.NewGuid().ToString("N"),
                ["oauth_signature_method"] = "HMAC-SHA1",
                ["oauth_timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ["oauth_version"] = "1.0",
                ["oauth_verifier"] = verifier
            };

            string signature = SignRequest(
                "GET",
                AccessTokenEndpointUrl,
                parameters,
                _consumerSecret,
                requestTokenSecret);

            parameters["oauth_signature"] = signature;

            // Build query string manually
            string url = AccessTokenEndpointUrl + "?" +
                         string.Join("&", parameters.Select(kvp =>
                             $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            var parts = HttpUtility.ParseQueryString(body);

            return new TokenInfo(parts["oauth_token"], parts["oauth_token_secret"]);
        }


        private static string SignRequest(
            string method,
            string url,
            SortedDictionary<string, string> parameters,
            string consumerSecret,
            string tokenSecret)
        {
            string baseString = string.Join("&",
                method,
                Uri.EscapeDataString(url),
                Uri.EscapeDataString(string.Join(
                    "&",
                    parameters
                        .Where(kvp => kvp.Key != null && kvp.Value != null)
                        .Select(kvp =>$"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"))));

            string key = $"{Uri.EscapeDataString(consumerSecret)}&{Uri.EscapeDataString(tokenSecret ?? "")}";
            using var hmac = new HMACSHA1(Encoding.ASCII.GetBytes(key));
            return Convert.ToBase64String(hmac.ComputeHash(Encoding.ASCII.GetBytes(baseString)));
        }
    }

}
