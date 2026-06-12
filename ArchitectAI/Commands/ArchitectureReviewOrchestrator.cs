using ArchitectAI.Interfaces;
using ArchitectAI.Models;
using ArchitectAI.Services;

namespace ArchitectAI.Commands;

public class ArchitectureReviewOrchestrator 
{
    private readonly SecurityAgentService _securityAgent;
    private readonly ReliabilityAgentService _reliabilityAgent;
    //private readonly ChiefArchitectAgentService _chiefArchitectAgent;
    private readonly IChiefArchitectAgentService _chiefArchitectAgent;
    private readonly IntegrationAgentService _integrationAgent;
    private readonly ComplianceAgentService _complianceAgent;
    

    /// <summary>
    /// Initializes the orchestrator with all required specialist and chief architect agent services.
    /// </summary>
    public ArchitectureReviewOrchestrator(
        SecurityAgentService securityAgent,
        ReliabilityAgentService reliabilityAgent,
        //ChiefArchitectAgentService chiefArchitectAgent,
        IChiefArchitectAgentService chiefArchitectAgent,
        IntegrationAgentService integrationAgent,
        ComplianceAgentService complianceAgent)
    {
        _securityAgent = securityAgent;
        _reliabilityAgent = reliabilityAgent;
        _chiefArchitectAgent = chiefArchitectAgent;
        _integrationAgent = integrationAgent;
        _complianceAgent = complianceAgent;
    }

    /// <summary>
    /// Executes all specialist agent reviews in parallel, then consolidates the results
    /// via the Chief Architect agent and returns the final architecture review result.
    /// </summary>
    public async Task<ArchitectureReviewResult> RunAsync(ArchitectureReviewRequest request)
    {
        var input = $"""
        Project: {request.ProjectName}

        Architecture Description:
        {request.ArchitectureDescription}

        Mermaid Diagram:
        {request.MermaidDiagram}

        OpenAPI Spec:
        {request.OpenApiSpec}

        Requirements:
        {request.Requirements}
        """;

        var securityTask = _securityAgent.ReviewAsync(input);
        var integrationTask = _integrationAgent.ReviewAsync(input);
        var reliabilityTask = _reliabilityAgent.ReviewAsync(input);
        var complianceTask = _complianceAgent.ReviewAsync(input);

        await Task.WhenAll(securityTask, integrationTask, reliabilityTask, complianceTask);

        var agents = new List<AgentReviewResult>
        {
            securityTask.Result,
            integrationTask.Result,
            reliabilityTask.Result,
            complianceTask.Result
        };

     //var chiefReview = await _chiefArchitectAgent.ReviewAsync(request, agents);
          var chiefReview = await _chiefArchitectAgent.ConsolidateAsync(request, agents);

        var topRisks = agents
            .SelectMany(a => a.Findings.Select(f => new TopRisk
            {
                Title = f.Title,
                Severity = f.Severity,
                OwnerAgent = a.AgentName
            }))
            .Take(3)
            .ToList();

        var recommendedActions = agents
            .SelectMany(a => a.Findings)
            .Take(3)
            .Select((f, index) => new RecommendedAction
            {
                Priority = index + 1,
                Action = f.Recommendation,
                Impact = f.Description
            })
            .ToList();

        var averageScore = (int)agents.Average(a => a.RiskScore);

        return new ArchitectureReviewResult
        {
            ReviewId = $"REV-{DateTime.UtcNow:yyyyMMddHHmmss}",
            ProjectName = request.ProjectName,
            OverallRisk = chiefReview.OverallRisk,
            OverallScore = chiefReview.OverallScore,
            Summary = chiefReview.Summary,
            Agents = agents,
            TopRisks = chiefReview.TopRisks,
            RecommendedActions = chiefReview.RecommendedActions,
            Roadmap = chiefReview.Roadmap,
            KnowledgeSources = chiefReview.KnowledgeSources
        };
    }
}