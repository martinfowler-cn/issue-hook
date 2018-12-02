using Octokit;

namespace IssueHookAPI.Services
{
    public class GitHubServices
    {
        public static readonly GitHubServices Instance = new GitHubServices();
        private GitHubClient client;
        private GitHubServices()
        {
            client = new GitHubClient(new ProductHeaderValue("my-cool-app"));
            var tokenAuth = new Credentials("e3289c00a8b3fa49eb677277229ff18074376945"); // NOTE: not real token
            client.Credentials = tokenAuth;
        }

        public Issue CreateIssue(int repositoryId,NewIssue issue)
        {
            return client.Issue.Create(repositoryId,issue).Result;                 
        }

        public Issue GetIssuebyId(int repositoryId, int issueNumber)
        {
            return client.Issue.Get(repositoryId, issueNumber).Result;
        }

        public Issue UpdateIssue(int repositoryId, int issueId, IssueUpdate issueUpdate)
        {
            return client.Issue.Update(repositoryId, issueId, issueUpdate).Result;
        }
    }
}