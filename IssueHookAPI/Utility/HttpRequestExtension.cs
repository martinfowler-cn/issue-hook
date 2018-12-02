using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Octokit;

namespace IssueHookAPI.Utility
{
    public static class HttpRequestExtension
    {
        public static JObject tryGetData(this HttpRequest request)
        {
            if (request.Body?.Length > 0)
            {
                return null;
            }

            return null;
        }
    }
}