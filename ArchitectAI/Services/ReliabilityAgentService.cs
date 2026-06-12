using ArchitectAI.Configuration;
using ArchitectAI.Models;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System.Text.Json;

namespace ArchitectAI.Services;

public class ReliabilityAgentService
{
    private readonly AzureOpenAIRestClient _client;

    public ReliabilityAgentService(AzureOpenAIRestClient client)
    {
        _client = client;
    }

    public async Task<AgentReviewResult> ReviewAsync(string architecture)
    {
        var systemPrompt = """
        YYou are a Cloud Reliability Architecture Review Agent.
        Rules:
        - riskScore MUST be an integer between 0 and 100.
        - 0 means no risk.
        - 50 means moderate risk.
        - 100 means critical risk.

        Review the provided architecture and identify reliability risks related to:
        - retry policies
        - dead-letter queues
        - poison message handling
        - monitoring and alerting
        - observability
        - scalability bottlenecks
        - failure recovery
        - single points of failure

        Return ONLY valid JSON.

        {
          "agentName": "Reliability Agent",
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
        """;

        var json = await _client.CompleteChatAsync(systemPrompt, architecture);

        var result = JsonSerializer.Deserialize<AgentReviewResult>(
            CleanJson(json),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (result == null)
            throw new Exception("Unable to deserialize Reliability Agent response.");

        Normalize(result, "Reliability Agent");

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
    /*  private readonly AzureOpenAISettings _settings;

      public ReliabilityAgentService(IOptions<AzureOpenAISettings> settings)
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
              You are a Cloud Reliability Architecture Review Agent.
              Rules:
              - riskScore MUST be an integer between 0 and 100.
              - 0 means no risk.
              - 50 means moderate risk.
              - 100 means critical risk.

              Review the provided architecture and identify reliability risks related to:
              - retry policies
              - dead-letter queues
              - poison message handling
              - monitoring and alerting
              - observability
              - scalability bottlenecks
              - failure recovery
              - single points of failure

              Return ONLY valid JSON.

              {
                "agentName": "Reliability Agent",
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
              throw new Exception("Unable to deserialize Reliability Agent response.");

          result.Status = "Completed";
          result.FindingsCount = result.Findings.Count;

          return result;
      }
    */
}