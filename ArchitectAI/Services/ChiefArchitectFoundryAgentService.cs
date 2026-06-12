using ArchitectAI.Configuration;
using ArchitectAI.DTO;
using ArchitectAI.Interfaces;
using ArchitectAI.Models;
using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.Identity;
using Microsoft.Extensions.Options;
using OpenAI.Responses;
using System.Text.Json;

namespace ArchitectAI.Services;


public class ChiefArchitectFoundryAgentService : IChiefArchitectAgentService
{
    private readonly FoundrySettings _settings;

    public ChiefArchitectFoundryAgentService(IOptions<FoundrySettings> settings)
    {
        _settings = settings.Value;
    }
    /// <summary>
    /// Invokes an Azure AI Foundry agent to consolidate specialist findings into a
    /// structured executive review, returning a <see cref="ChiefArchitectReviewResult"/>.
    /// </summary>
    public Task<ChiefArchitectReviewResult> ConsolidateAsync(
            ArchitectureReviewRequest request,
            List<AgentReviewResult> agents)
    {
#pragma warning disable OPENAI001

        string endpoint = _settings.Endpoint;
        string agentName = _settings.AgentName;
        string agentVersion = _settings.AgentVersion;


        AIProjectClient projectClient = new(
    new Uri(endpoint),
    new AzureCliCredential());

        var prompt = $"""
Architecture Description:

{request.ArchitectureDescription}

Mermaid Diagram:

{request.MermaidDiagram}

Requirements:

{request.Requirements}

Please perform a complete architecture review and return JSON only.
""";

        AgentReference agentReference = new(name: agentName, version: agentVersion);
        ProjectResponsesClient responseClient = projectClient.ProjectOpenAIClient.GetProjectResponsesClientForAgent(agentReference);
        // Use the agent to generate a response
        ResponseResult response = responseClient.CreateResponse(prompt);


        var json = response.GetOutputText();
        

        var result = JsonSerializer.Deserialize<ChiefArchitectReviewResult>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
 
        if (result == null)
            throw new Exception("Unable to deserialize Foundry Chief Architect Agent response.");

        return Task.FromResult(result);

    }
}
/*using ArchitectAI.Configuration;
using ArchitectAI.DTO;
using ArchitectAI.Interfaces;
using ArchitectAI.Models;
using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Options;
using OpenAI.Responses;
using System.Text.Json;

#pragma warning disable OPENAI001

namespace ArchitectAI.Services;


public class ChiefArchitectFoundryAgentService : IChiefArchitectAgentService
{
    private readonly FoundrySettings _settings;

    public ChiefArchitectFoundryAgentService(IOptions<FoundrySettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<ChiefArchitectReviewResult> ConsolidateAsync(
        ArchitectureReviewRequest request,
        List<AgentReviewResult> agents)
    {
        string agentName = _settings.AgentName;
        string agentVersion = _settings.AgentVersion;

        AIProjectClient projectClient = new(
            endpoint: new Uri(_settings.Endpoint),
            tokenProvider: new DefaultAzureCredential());

        AgentReference agentReference = new(
            name: agentName,
            version: agentVersion);

        ProjectResponsesClient responseClient =
            projectClient
                .GetProjectOpenAIClient()
                .GetProjectResponsesClientForAgent(agentReference);

        var input = JsonSerializer.Serialize(new
        {
            project = request,
            agentResults = agents
        });

        ResponseResult response = await responseClient.CreateResponseAsync(input);

        var json = response.GetOutputText();

        var result = JsonSerializer.Deserialize<ChiefArchitectReviewResult>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (result == null)
            throw new Exception("Unable to deserialize Foundry Chief Architect Agent response.");

        return result;
    }
#pragma warning disable OPENAI001

}

*/