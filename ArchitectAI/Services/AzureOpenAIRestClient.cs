 
using global::ArchitectAI.Configuration;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace ArchitectAI.Services;

public class AzureOpenAIRestClient
{
    private readonly AzureOpenAISettings _settings;
    private readonly HttpClient _httpClient;

    public AzureOpenAIRestClient(
        IOptions<AzureOpenAISettings> settings,
        IHttpClientFactory httpClientFactory)
    {
        _settings = settings.Value;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<string> CompleteChatAsync(string systemPrompt, string userPrompt)
    {
        var url =
            $"{_settings.Endpoint.TrimEnd('/')}/openai/deployments/{_settings.DeploymentName}/chat/completions?api-version=2024-10-21";

        var payload = new
        {
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature = 0.2,
            max_tokens = 1500
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("api-key", _settings.ApiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(request);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Azure OpenAI REST error: {response.StatusCode} - {responseText}");

        using var doc = JsonDocument.Parse(responseText);

        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "";
    }
}