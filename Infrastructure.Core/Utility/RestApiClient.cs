using System.Net.Http.Headers;
using System.Text;

using Newtonsoft.Json;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public class TokenResponse
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    // Add other properties if needed (e.g., refresh_token, expires_in)
}

public class RestApiClient
{
    protected readonly HttpClient _httpClient;

    public RestApiClient(string baseUrl, string token = null)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };

        SetAccessToken(token);
    }

    public void SetAccessToken(string token)
    {
        if (token.IsNotNullOrEmpty())
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public virtual async Task<TResponse> GetTokenAsync<TResponse>(string endpoint, string username, string password)
        where TResponse : class
    {
        var requestBody = new
        {
            Email = username,
            Password = password
        };

        var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(endpoint, jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.Content.Headers.ContentType.MediaType == "application/json")
        {
            var tokenResponse = JsonConvert.DeserializeObject<TResponse>(responseContent);
            return tokenResponse;
        }
        else if (response.Content.Headers.ContentType.MediaType == "text/plain")
        {
            return responseContent as TResponse;
        }

        return null;
    }

    public async Task<TResponse> GetAsync<TResponse>(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<TResponse>(content);
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest content)
    {
        var jsonContent = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(endpoint, jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<TResponse>(responseContent);
    }
}