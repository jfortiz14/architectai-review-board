using ArchitectAI.Models;

namespace ArchitectAI.Interfaces;

public interface IArchitectureAgent
{
    Task<AgentReviewResult> ReviewAsync(
        ArchitectureReviewRequest request);
}
