using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IssueHookAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks;
using Newtonsoft.Json.Linq;
using Octokit;

namespace IssueHookAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HookController: ControllerBase
    {

        [GitHubWebHook(EventName = "issues")]
        public IActionResult HandlerForIssueOpened([FromBody] JObject data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            if (data["action"].ToString() == "opened")
            {
                
            }

            return Ok();
        }

        [GitHubWebHook(EventName = "issue_comment")]
        public IActionResult HandlerForIssueComment([FromBody] JObject data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            if (data["action"].ToString() == "created")
            {
                if (string.Equals(data["comment"]["body"].ToString(), CONSTS.Command.Cmd_Accept, StringComparison.CurrentCultureIgnoreCase)
                    && !data["issue"]["assignee"].HasValues)
                {

                    var repositoryId = int.Parse(data["repository"]["id"].ToString());
                    var issueNumber = int.Parse(data["issue"]["number"].ToString());
                    
                    var targetIssue = GitHubServices.Instance.GetIssuebyId(
                        repositoryId, issueNumber);
                    var updateDefi = targetIssue.ToUpdate();
                    updateDefi.RemoveLabel(CONSTS.Label.Label_Welcome);
                    updateDefi.AddLabel(CONSTS.Label.Label_Translating);
                    updateDefi.AddAssignee(data["comment"]["user"]["login"].ToString());

                    GitHubServices.Instance.UpdateIssue(repositoryId, issueNumber, updateDefi);
                }
            }

            return Ok();
        }
    }
}