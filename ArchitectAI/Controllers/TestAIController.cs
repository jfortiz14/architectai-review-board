
using Microsoft.AspNetCore.Mvc;

namespace ArchitectAI.Controllers;

[ApiController]
[Route("api/test-ai")]
public class TestAIController : ControllerBase
{
    private readonly SecurityAgentService _agent;

    public TestAIController(SecurityAgentService agent)
    {
        _agent = agent;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] string architecture)
    {
        var result = await _agent.ReviewAsync(architecture);

        return Ok(result);
    }
}