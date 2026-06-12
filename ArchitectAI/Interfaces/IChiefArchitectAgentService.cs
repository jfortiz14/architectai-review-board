using ArchitectAI.DTO;
using ArchitectAI.Models;

namespace ArchitectAI.Interfaces;

public interface IChiefArchitectAgentService
{
    Task<ChiefArchitectReviewResult> ConsolidateAsync(
        ArchitectureReviewRequest request,
        List<AgentReviewResult> agents);
}