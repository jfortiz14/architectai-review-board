using ArchitectAI.Models;
using System.Text.Json;

namespace ArchitectAI.Services;

public class ComplianceAgentService
{
    private readonly AzureOpenAIRestClient _client;

    public ComplianceAgentService(AzureOpenAIRestClient client)
    {
        _client = client;
    }
    public async Task<AgentReviewResult> ReviewAsync(string architecture)
    {
        var systemPrompt = """
        You are a Healthcare Compliance Architecture Review Agent.

        Rules:
        - riskScore MUST be an integer between 0 and 100.
        - 0 means no risk.
        - 50 means moderate risk.
        - 100 means critical risk.

        Review the provided architecture and identify compliance risks related to:
        - HIPAA-style safeguards
        - PHI/PII handling
        - audit logging
        - access traceability
        - data retention
        - encryption at rest
        - encryption in transit
        - least privilege access
        - vendor/partner access
        - breach investigation readiness

        Return ONLY valid JSON.

        {
          "agentName": "Compliance Agent",
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
        - Do not provide legal advice.
        - Focus on architecture controls and risk patterns.
        """;

        var json = await _client.CompleteChatAsync(systemPrompt, architecture);

        var result = JsonSerializer.Deserialize<AgentReviewResult>(
            CleanJson(json),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (result == null)
            throw new Exception("Unable to deserialize Compliance Agent response.");

        Normalize(result, "Compliance Agent");

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

    public ComplianceAgentService(IOptions<AzureOpenAISettings> settings)
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
            You are a Healthcare Compliance Architecture Review Agent.

            Rules:
            - riskScore MUST be an integer between 0 and 100.
            - 0 means no risk.
            - 50 means moderate risk.
            - 100 means critical risk.

            Review the provided architecture and identify compliance risks related to:
            - HIPAA-style safeguards
            - PHI/PII handling
            - audit logging
            - access traceability
            - data retention
            - encryption at rest
            - encryption in transit
            - least privilege access
            - vendor/partner access
            - breach investigation readiness

            Return ONLY valid JSON.

            {
              "agentName": "Compliance Agent",
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
            - Do not provide legal advice.
            - Focus on architecture controls and risk patterns.
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
            throw new Exception("Unable to deserialize Compliance Agent response.");

        result.Status = "Completed";
        result.FindingsCount = result.Findings.Count;

        return result;
    }
    */
}