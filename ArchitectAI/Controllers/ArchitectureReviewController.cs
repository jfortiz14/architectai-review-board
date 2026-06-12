
using ArchitectAI.Commands;
using global::ArchitectAI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ArchitectAI.Controllers;

 
[ApiController]
[Route("api/[controller]")]
public class ArchitectureReviewController : ControllerBase
{
    private readonly ArchitectureReviewOrchestrator _orchestrator;

    /// <summary>
    /// Initializes the controller with the architecture review orchestrator.
    /// </summary>
    public ArchitectureReviewController(ArchitectureReviewOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    /// <summary>
    /// Accepts an architecture review request and returns the full multi-agent review result.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ArchitectureReviewResult>> CreateReview(
    [FromBody] ArchitectureReviewRequest request)
    {
        var result = await _orchestrator.RunAsync(request);
        return Ok(result);
    }
    /*
    [HttpPost]
    public ActionResult<ArchitectureReviewResult> CreateReviewMock(
        [FromBody] ArchitectureReviewRequest request)
    {
        var result = new ArchitectureReviewResult
        {
            ReviewId = $"REV-{DateTime.UtcNow:yyyyMMddHHmmss}",
            ProjectName = string.IsNullOrWhiteSpace(request.ProjectName)
                ? "Untitled Architecture Review"
                : request.ProjectName,
            OverallRisk = "Medium",
            OverallScore = 68,
            Summary = "The architecture is functional but has security, reliability, and compliance risks that should be addressed before production exposure.",
            Agents =
            [
                new AgentReviewResult
                {
                    AgentName = "Security Agent",
                    RiskScore = 72,
                    FindingsCount = 3,
                    Findings =
                    [
                        new Finding
                        {
                            Title = "Shared API key authentication",
                            Severity = "High",
                            Category = "Authentication",
                            Description = "The architecture relies on shared API keys for external partners, which can increase the risk of credential leakage and weak tenant isolation.",
                            Recommendation = "Use OAuth 2.0 Client Credentials with per-partner application registration, scoped permissions, and key rotation."
                        }
                    ]
                },
                new AgentReviewResult
                {
                    AgentName = "Integration Agent",
                    RiskScore = 61,
                    FindingsCount = 2,
                    Findings =
                    [
                        new Finding
                        {
                            Title = "Webhook dependency without versioning strategy",
                            Severity = "Medium",
                            Category = "API Versioning",
                            Description = "The architecture uses webhooks but does not describe schema versioning or backward compatibility handling.",
                            Recommendation = "Introduce versioned event contracts and maintain backward-compatible payload changes."
                        }
                    ]
                },
                new AgentReviewResult
                {
                    AgentName = "Reliability Agent",
                    RiskScore = 75,
                    FindingsCount = 2,
                    Findings =
                    [
                        new Finding
                        {
                            Title = "Missing dead-letter queue strategy",
                            Severity = "High",
                            Category = "Resilience",
                            Description = "Failed messages may be lost or repeatedly retried without visibility.",
                            Recommendation = "Use Azure Service Bus with retry policy, dead-letter queue, poison message handling, and alerting."
                        }
                    ]
                },
                new AgentReviewResult
                {
                    AgentName = "Compliance Agent",
                    RiskScore = 70,
                    FindingsCount = 2,
                    Findings =
                    [
                        new Finding
                        {
                            Title = "Sensitive patient data exposure",
                            Severity = "High",
                            Category = "PHI/PII",
                            Description = "The intake flow includes demographics, insurance information, and uploaded documents, which may contain sensitive data.",
                            Recommendation = "Apply PHI minimization, encryption at rest and in transit, audit logging, and role-based access controls."
                        }
                    ]
                }
            ],
            TopRisks =
            [
                new TopRisk
                {
                    Title = "Shared API key authentication",
                    Severity = "High",
                    OwnerAgent = "Security Agent"
                },
                new TopRisk
                {
                    Title = "Missing dead-letter queue strategy",
                    Severity = "High",
                    OwnerAgent = "Reliability Agent"
                },
                new TopRisk
                {
                    Title = "Sensitive patient data exposure",
                    Severity = "High",
                    OwnerAgent = "Compliance Agent"
                }
            ],
            RecommendedActions =
            [
                new RecommendedAction
                {
                    Priority = 1,
                    Action = "Replace shared API keys with OAuth 2.0 Client Credentials.",
                    Impact = "Improves partner isolation, access control, and credential governance."
                },
                new RecommendedAction
                {
                    Priority = 2,
                    Action = "Add Azure Service Bus retry and dead-letter queue handling.",
                    Impact = "Improves operational reliability and failure visibility."
                },
                new RecommendedAction
                {
                    Priority = 3,
                    Action = "Add audit logs for patient data access and partner API calls.",
                    Impact = "Improves compliance readiness and traceability."
                }
            ]
        };

        return Ok(result);
    }
    */
}