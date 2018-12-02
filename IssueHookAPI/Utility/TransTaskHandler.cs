using Octokit;

namespace IssueHookAPI.Utility
{
    public class TransTaskHandler
    {
        public NewIssue CreateTransTaskIssue(string title)
        {
            var createIssue = new NewIssue(title);
            createIssue.Labels.Add(CONSTS.Label.Label_Welcome);
            return createIssue;
        }
    }
}