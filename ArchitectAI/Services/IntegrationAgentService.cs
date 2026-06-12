using ArchitectAI.Configuration;
using ArchitectAI.Models;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System.Text.Json;

namespace ArchitectAI.Services;

public class IntegrationAgentService
{
    private readonly AzureOpenAIRestClient _client;

    public IntegrationAgentService(AzureOpenAIRestClient client)
    {
        _client = client;
    }
    public async Task<AgentReviewResult> ReviewAsync(string architecture)
    {
        var systemPrompt = """
        You are an Enterprise Integration Architecture Review Agent.
        Rules:
        - riskScore MUST be an integer between 0 and 100.
        - 0 means no risk.
        - 50 means moderate risk.
        - 100 means critical risk.

        Review the provided architecture and identify integration risks related to:
        - API versioning
        - breaking changes
        - webhook design
        - event-driven architecture
        - idempotency
        - contract governance
        - external partner dependencies
        - schema evolution
        - backward compatibility
        - integration observability

        Return ONLY valid JSON.

        {
          "agentName": "Integration Agent",
          "status": "Completed",
          "riskScore": number,
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
        - Do not invent information.
        - Be practical and specific.
        """;

        var json = await _client.CompleteChatAsync(systemPrompt, architecture);

        var result = JsonSerializer.Deserialize<AgentReviewResult>(
            CleanJson(json),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (result == null)
            throw new Exception("Unable to deserialize Integration Agent response.");

        Normalize(result, "Integration Agent");

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
    private readonly AzureOpenAISettings _settings;

    public IntegrationAgentService(IOptions<AzureOpenAISettings> settings)
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
            You are an Enterprise Integration Architecture Review Agent.
            Rules:
            - riskScore MUST be an integer between 0 and 100.
            - 0 means no risk.
            - 50 means moderate risk.
            - 100 means critical risk.

            Review the provided architecture and identify integration risks related to:
            - API versioning
            - breaking changes
            - webhook design
            - event-driven architecture
            - idempotency
            - contract governance
            - external partner dependencies
            - schema evolution
            - backward compatibility
            - integration observability

            Return ONLY valid JSON.

            {
              "agentName": "Integration Agent",
              "status": "Completed",
              "riskScore": number,
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
            - Do not invent information.
            - Be practical and specific.
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
            throw new Exception("Unable to deserialize Integration Agent response.");

        result.Status = "Completed";
        result.FindingsCount = result.Findings.Count;

        return result;
    }
    */
}