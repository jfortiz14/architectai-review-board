using ArchitectAI.Models;

namespace ArchitectAI.DTO;

public class ChiefArchitectReviewResult
{
    public string Summary { get; set; } = "";
    public string OverallRisk { get; set; } = "";
    public int OverallScore { get; set; }
    public List<TopRisk> TopRisks { get; set; } = new();
    public List<RecommendedAction> RecommendedActions { get; set; } = new();
    public ArchitectureRoadmap Roadmap { get; set; } = new();
    public List<string> KnowledgeSources { get; set; } = [];
}

