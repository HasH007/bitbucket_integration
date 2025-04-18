using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace BitbucketIntegration.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestCodeGeneratorController : ControllerBase
    {
        [HttpPost("processrepo")]
        public IActionResult ProcessRepo([FromBody] RepoInfo repoInfo)
        {
            // TODO: Implement the actual processing logic
            return Ok(new { message = "Repository information received successfully", data = repoInfo });
        }
    }

    public class RepoInfo
    {
        public string Repository { get; set; }
        public string BranchName { get; set; }
        public string ProjectType { get; set; }
        public List<ReferredRepo> ReferredRepos { get; set; } = new List<ReferredRepo>();
    }

    public class ReferredRepo
    {
        public string Repository { get; set; }
        public string BranchName { get; set; }
    }
} 