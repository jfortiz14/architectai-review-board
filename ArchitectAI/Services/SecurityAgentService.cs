using ArchitectAI.Configuration;
using ArchitectAI.Models;
using ArchitectAI.Services;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System.Text.Json;

public class SecurityAgentService
{

    private readonly AzureOpenAIRestClient _client;

    public SecurityAgentService(AzureOpenAIRestClient client)
    {
        _client = client;
    }
    public async Task<AgentReviewResult> ReviewAsync(string architecture)
    {
        var systemPrompt = """
        You are a Security Architecture Review Agent.
        Rules:
        - riskScore MUST be an integer between 0 and 100.
        - 0 means no risk.
        - 50 means moderate risk.
        - 100 means critical risk.
        Return ONLY valid JSON.

        {
          "agentName": "Security Agent",
          "status": "Completed",
          "riskScore": 0,
          "findings": [
            {
              "title": "",
              "severity": "Low|Medium|High|Critical",
              "category": "",
              "description": "",
              "recommendation": ""
            }
          ]
        }

        Rules:
        - Return only JSON.
        - Do not wrap JSON in markdown.
        - Limit findings to 3.
        - riskScore MUST be an integer between 0 and 100.
        - 0 means no risk.
        - 50 means moderate risk.
        - 100 means critical risk.
        """;

        var json = await _client.CompleteChatAsync(systemPrompt, architecture);

        var result = JsonSerializer.Deserialize<AgentReviewResult>(
            CleanJson(json),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (result == null)
            throw new Exception("Unable to deserialize Security Agent response.");

        Normalize(result, "Security Agent");

        return result;
    }

    private static void Normalize(AgentReviewResult result, string agentName)
    {
        result.AgentName = string.IsNullOrWhiteSpace(result.AgentName)
            ? agentName
            : result.AgentName;

        result.Status = "Completed";
        result.FindingsCount = result.Findings?.Count ?? 0;

        if (result.RiskScore > 0 && result.RiskScore <= 10)
            result.RiskScore *= 10;

        result.RiskScore = Math.Clamp(result.RiskScore, 0, 100);
    }

    private static string CleanJson(string text)
    {
        return text
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();
    }
    /*
     * 
     *    //private readonly AzureOpenAISettings _settings;
    public SecurityAgentService(IOptions<AzureOpenAISettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<AgentReviewResult> ReviewAsync(string architecture)
    {
        var azureClient = new AzureOpenAIClient(
            new Uri(_settings.Endpoint),
            new AzureKeyCredential(_settings.ApiKey));

        ChatClient chatClient = azureClient.GetChatClient(_settings.DeploymentName);

        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage("""
            You are a Security Architecture Review Agent.
            Rules:
            - riskScore MUST be an integer between 0 and 100.
            - 0 means no risk.
            - 50 means moderate risk.
            - 100 means critical risk.

            Return ONLY valid JSON.

            {
              "agentName": "Security Agent",
              "riskScore": number,
              "findings": [
                {
                  "title": "",
                  "severity": "",
                  "description": "",
                  "recommendation": ""
                }
              ]
            }
            """),
            ChatMessage.CreateUserMessage(architecture)
        };

        var response = await chatClient.CompleteChatAsync(messages);

        var json = response.Value.Content[0].Text;

        var result = JsonSerializer.Deserialize<AgentReviewResult>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (result == null)
        {
            throw new Exception("Unable to deserialize Security Agent response.");
        }

        result.FindingsCount = result.Findings.Count;

        return result;
    }
    */
}