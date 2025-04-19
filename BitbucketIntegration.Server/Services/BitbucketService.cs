using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace BitbucketIntegration.Server.Services
{
    public class BitbucketService
    {
        private readonly HttpClient _httpClient;
        private readonly string _bitbucketToken;
        private readonly string _workspace;
        private readonly string _baseUrl = "https://api.bitbucket.org/2.0";

        public BitbucketService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _bitbucketToken = configuration["Bitbucket:Token"];
            _workspace = configuration["Bitbucket:Workspace"];
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _bitbucketToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> CloneRepository(string repository, string branchName, string localPath)
        {
            try
            {
                // Extract repository name from the URL
                var repoName = ExtractRepoName(repository);
                
                // Create local directory if it doesn't exist
                var fullPath = Path.Combine(localPath, repoName);
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }

                // Get the latest commit hash for the branch
                var commitHash = await GetLatestCommitHash(repoName, branchName);
                if (string.IsNullOrEmpty(commitHash))
                {
                    throw new Exception($"Could not find branch {branchName} in repository {repoName}");
                }

                // Download the repository content
                await DownloadRepositoryContent(repoName, commitHash, fullPath);

                return fullPath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to clone repository: {ex.Message}", ex);
            }
        }

        private async Task<string> GetLatestCommitHash(string repoName, string branchName)
        {
            var url = $"{_baseUrl}/repositories/{_workspace}/{repoName}/refs/branches/{branchName}";
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get branch information: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var branchInfo = JsonSerializer.Deserialize<BranchInfo>(content);
            
            return branchInfo?.Target?.Hash;
        }

        private async Task DownloadRepositoryContent(string repoName, string commitHash, string localPath)
        {
            var url = $"{_baseUrl}/repositories/{_workspace}/{repoName}/src/{commitHash}/";
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get repository content: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var files = JsonSerializer.Deserialize<FileList>(content);

            foreach (var file in files.Values)
            {
                if (file.Type == "commit_file")
                {
                    await DownloadFile(repoName, commitHash, file.Path, localPath);
                }
            }
        }

        private async Task DownloadFile(string repoName, string commitHash, string filePath, string localPath)
        {
            var url = $"{_baseUrl}/repositories/{_workspace}/{repoName}/src/{commitHash}/{filePath}";
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to download file {filePath}: {response.StatusCode}");
            }

            var fileContent = await response.Content.ReadAsByteArrayAsync();
            var fullPath = Path.Combine(localPath, filePath);
            
            // Create directory if it doesn't exist
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            
            await File.WriteAllBytesAsync(fullPath, fileContent);
        }

        private string ExtractRepoName(string repositoryUrl)
        {
            // Handle both HTTPS and SSH URLs
            var parts = repositoryUrl.Split(new[] { '/', ':' }, StringSplitOptions.RemoveEmptyEntries);
            var lastPart = parts[^1];
            return lastPart.Replace(".git", "");
        }
    }

    public class BranchInfo
    {
        public TargetInfo Target { get; set; }
    }

    public class TargetInfo
    {
        public string Hash { get; set; }
    }

    public class FileList
    {
        public FileInfo[] Values { get; set; }
    }

    public class FileInfo
    {
        public string Type { get; set; }
        public string Path { get; set; }
    }
} 