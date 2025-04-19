using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using BitbucketIntegration.Server.Services;
using Microsoft.Extensions.Configuration;

namespace BitbucketIntegration.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestCodeGeneratorController : ControllerBase
    {
        private readonly BitbucketService _bitbucketService;
        private readonly string _localPath;

        public TestCodeGeneratorController(BitbucketService bitbucketService, IConfiguration configuration)
        {
            _bitbucketService = bitbucketService;
            _localPath = configuration["LocalPath"] ?? "C:\\Temp\\Repositories";
        }

        [HttpPost("processrepo")]
        public async Task<IActionResult> ProcessRepo([FromBody] RepoInfo repoInfo)
        {
            try
            {
                // Clone the primary repository
                var primaryRepoPath = await _bitbucketService.CloneRepository(
                    repoInfo.Repository,
                    repoInfo.BranchName,
                    _localPath
                );

                // Clone all referred repositories
                var referredRepoPaths = new List<string>();
                foreach (var referredRepo in repoInfo.ReferredRepos)
                {
                    var path = await _bitbucketService.CloneRepository(
                        referredRepo.Repository,
                        referredRepo.BranchName,
                        _localPath
                    );
                    referredRepoPaths.Add(path);
                }

                return Ok(new
                {
                    message = "Repositories cloned successfully",
                    primaryRepoPath,
                    referredRepoPaths
                });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = $"Error processing repositories: {ex.Message}" });
            }
        }
    }

    public class RepoInfo
    {
        public string Repository { get; set; }
        public string BranchName { get; set; }
        public string ProjectType { get; set; }
        public bool Primary { get; set; } = true;
        public List<ReferredRepo> ReferredRepos { get; set; } = new List<ReferredRepo>();
    }

    public class ReferredRepo
    {
        public string Repository { get; set; }
        public string BranchName { get; set; }
        public bool Primary { get; set; } = false;
    }
} 