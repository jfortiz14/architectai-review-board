using ArchitectAI.Configuration;
using ArchitectAI.DTO;
using ArchitectAI.Models;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System.Text.Json;

namespace ArchitectAI.Services;

public class ChiefArchitectAgentService  
{
    private readonly AzureOpenAISettings _settings;

    /// <summary>
    /// Initializes the service with Azure OpenAI configuration settings.
    /// </summary>
    public ChiefArchitectAgentService(IOptions<AzureOpenAISettings> settings)
    {
        _settings = settings.Value;
    }

    /// <summary>
    /// Sends all specialist agent findings to Azure OpenAI and returns a consolidated
    /// executive architecture review as a <see cref="ChiefArchitectReviewResult"/>.
    /// </summary>
    public async Task<ChiefArchitectReviewResult> ReviewAsync(
        ArchitectureReviewRequest request,
        List<AgentReviewResult> agentResults)
    {
        var azureClient = new AzureOpenAIClient(
            new Uri(_settings.Endpoint),
            new AzureKeyCredential(_settings.ApiKey));

        ChatClient chatClient = azureClient.GetChatClient(_settings.DeploymentName);

        var input = JsonSerializer.Serialize(new
        {
            project = request,
            agentResults
        });

        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage("""
            You are a Chief Software Architect Agent.

            Your job is to consolidate specialized architecture review findings into an executive architecture review.

            Analyze the provided agent results and return ONLY valid JSON.

            Required JSON shape:

            {
            	"summary": "",
            	"overallRisk": "Low|Medium|High|Critical",
            	"overallScore": number,
            	"topRisks": [
            		{
            			"title": "",
            			"severity": "Low|Medium|High|Critical",
            			"ownerAgent": ""
            		}
            	],
            	"recommendedActions": [
            		{
            			"priority": number,
            			"action": "",
            			"impact": ""
            		}
            	],
            	"roadmap": {
            		"quickWins": [
            			"Replace shared API keys with OAuth2",
            			"Enable audit logging",
            			"Implement webhook signing"
            		],
            		"strategicImprovements": [
            			"Adopt event-driven architecture",
            			"Introduce centralized observability",
            			"Implement zero-trust access controls"
            		],
            		"longTermVision": [
            			"AI-governed architecture reviews",
            			"Continuous architecture compliance",
            			"Self-healing integration platform"
            		]
            	}
            }

            Rules:
            - Return only JSON.
            - Do not wrap JSON in markdown.
            - Do not invent findings that are not supported by the agent results.
            - Limit topRisks to 3.
            - Limit recommendedActions to 3.
            - overallScore must be between 0 and 100.
            - Higher score means higher risk.
            - Make the summary clear, executive, and demo-friendly.
            """),
            ChatMessage.CreateUserMessage(input)
        };

        var response = await chatClient.CompleteChatAsync(messages);
        var json = response.Value.Content[0].Text;

        var result = JsonSerializer.Deserialize<ChiefArchitectReviewResult>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (result == null)
            throw new Exception("Unable to deserialize Chief Architect Agent response.");

        return result;
    }
}