namespace ArchitectAI.Models;

public class ArchitectureReviewRequest
{
    public string ProjectName { get; set; } = string.Empty;
    public string ArchitectureDescription { get; set; } = string.Empty;
    public string MermaidDiagram { get; set; } = string.Empty;
    public string OpenApiSpec { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
}