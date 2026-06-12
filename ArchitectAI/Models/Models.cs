namespace ArchitectAI.Models;

public class ArchitectureReviewResult
{
    public string ReviewId { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string OverallRisk { get; set; } = string.Empty;
    public int OverallScore { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<AgentReviewResult> Agents { get; set; } = new();
    public List<TopRisk> TopRisks { get; set; } = new();
    public List<RecommendedAction> RecommendedActions { get; set; } = new();

    public ArchitectureRoadmap Roadmap { get; set; } = new();
    public List<string> KnowledgeSources { get; set; } = new();
}

public class AgentReviewResult
{
    public string AgentName { get; set; } = "";
    public string Status { get; set; } = "Completed";
    public int RiskScore { get; set; }
    public int FindingsCount { get; set; }
    public List<Finding> Findings { get; set; } = new();
}

public class Finding
{
    public string Title { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}

public class TopRisk
{
    public string Title { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string OwnerAgent { get; set; } = string.Empty;
}

public class RecommendedAction
{
    public int Priority { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
}

public class ArchitectureRoadmap
{
    public List<string> QuickWins { get; set; } = new();
    public List<string> StrategicImprovements { get; set; } = new();
    public List<string> LongTermVision { get; set; } = new();
}