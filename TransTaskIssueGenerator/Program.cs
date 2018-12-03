using System;

namespace TransTaskIssueGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
             ContentIndexService.Instance.BuildIndex();
             ContentIndexService.Instance.CreateIssue();
        }
    }
}