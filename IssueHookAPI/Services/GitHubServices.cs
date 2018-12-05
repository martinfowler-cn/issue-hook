using System;
using System.Threading;
using Octokit;

namespace IssueHookAPI.Services
{
    public class GitHubServices
    {
        public static readonly GitHubServices Instance = new GitHubServices();
        private GitHubClient client;
        
        private bool EnsureDistinctIssue = true;
//        private bool EnsureDistinctIssue = false; //WARN: Only Uncomment this line when you understand the meaning behind.
        
        private GitHubServices()
        {
            client = new GitHubClient(new ProductHeaderValue("TransBroker"));
            var tokenAuth = new Credentials(Environment.GetEnvironmentVariable("GITHUB_PERSONAL_TOKEN"));
            client.Credentials = tokenAuth;
        }

        public Issue CreateIssue(NewIssue issue)
        {
            return client.Issue.Create("martinfowler-cn","trans-tasks",issue).Result;                 
        }

        public Issue GetIssuebyId(int repositoryId, int issueNumber)
        {
            return client.Issue.Get(repositoryId, issueNumber).Result;
        }

        public Issue UpdateIssue(int repositoryId, int issueId, IssueUpdate issueUpdate)
        {
            return client.Issue.Update(repositoryId, issueId, issueUpdate).Result;
        }

        public bool IsIssueExist(string title, string href)
        {
            if (!EnsureDistinctIssue)
            {
                return false;
            }
            
            var request = new SearchIssuesRequest(title);
            request.Repos.Add("martinfowler-cn/trans-tasks");
            request.In = new[] {
                IssueInQualifier.Title
            };
            request.Type = IssueTypeQualifier.Issue;

            request.PerPage = 30;
            var issuesResult = client.Search.SearchIssues(request).Result;
            if (issuesResult.TotalCount > 0)
            {
                foreach (var issue in issuesResult.Items)
                {
                    if (issue.Body.StartsWith(href))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        public void CheckAPIRateLimit()
        {
            var miscellaneousRateLimit = client.Miscellaneous.GetRateLimits().Result;
            var coreRateLimit = miscellaneousRateLimit.Resources.Core;
            var howManyCoreRequestsCanIMakePerHour = coreRateLimit.Limit;
            var howManyCoreRequestsDoIHaveLeft = coreRateLimit.Remaining;
            var whenDoesTheCoreLimitReset = coreRateLimit.Reset; // UTC time

            // the "search" object provides your rate limit status for the Search API.
            var searchRateLimit = miscellaneousRateLimit.Resources.Search;

            var howManySearchRequestsCanIMakePerMinute = searchRateLimit.Limit;
            var howManySearchRequestsDoIHaveLeft = searchRateLimit.Remaining;
            var whenDoesTheSearchLimitReset = searchRateLimit.Reset; // UTC time            

            Console.WriteLine("API Limit Status: Total:{0} Left:{1}",howManySearchRequestsCanIMakePerMinute,howManySearchRequestsDoIHaveLeft);
            if (howManySearchRequestsDoIHaveLeft <=1)
            {
                Console.WriteLine("API Search Limit Threshold Reached. Current Time: {0} Time to wait: {1} sec",
                    DateTime.Now.ToString(), whenDoesTheSearchLimitReset.ToLocalTime().ToString());
                var sleepTicks = whenDoesTheSearchLimitReset.LocalDateTime.Ticks - DateTime.Now.Ticks;
                if (sleepTicks > 0)
                {
                    Thread.Sleep(TimeSpan.FromTicks(sleepTicks));
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromSeconds(15));
                }
            }
        }        
    }
}