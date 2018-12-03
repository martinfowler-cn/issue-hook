using Octokit;

namespace IssueHookAPI.Services
{
    public class GitHubServices
    {
        public static readonly GitHubServices Instance = new GitHubServices();
        private GitHubClient client;
        private GitHubServices()
        {
            client = new GitHubClient(new ProductHeaderValue("TransBroker"));
            var tokenAuth = new Credentials("e4804bb4dad02fa54c62ac98eecdb8bf73575566"); // NOTE: not real token
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