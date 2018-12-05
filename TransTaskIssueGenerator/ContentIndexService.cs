using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using AngleSharp;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;
using IssueHookAPI;
using IssueHookAPI.Services;
using Newtonsoft.Json;
using Octokit;

namespace TransTaskIssueGenerator
{
    public class ContentIndexService
    {
        public static readonly ContentIndexService Instance = new ContentIndexService();
        private string contentsJsonFileName = "Contents.json";
        private string contentsCreatedJsonFileName = "ContentsCreated.json";
        private Dictionary<string, TransAsset> _Contents = new Dictionary<string, TransAsset>();
        private Dictionary<string, TransAsset> _ContentsCreated = new Dictionary<string, TransAsset>();
        private string[] _canTransTypes = new[] {"article", "bliki",null};
        private ContentIndexService()
        {
        }

        public void BuildIndex()
        {
            if ( string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DONOTCACHECONTENTINDEX")) && File.Exists(contentsJsonFileName))
            {
                _Contents = JsonConvert.DeserializeObject<Dictionary<string, TransAsset>>(File.ReadAllText(contentsJsonFileName));
                Console.WriteLine("[WARN]Contents.json Exist, Use Cache Index Directly.");
                return;
            }
            
            var config = Configuration.Default.WithDefaultLoader();
            var document =  BrowsingContext.New(config).OpenAsync("https://martinfowler.com/tags/").Result;            
           
            var tags = document.QuerySelectorAll("div.tags p a");
            foreach (var item in tags)
            {
                Console.WriteLine("Current Tag Page Downloading========================>{0}",item.TextContent);
                if (item.TextContent == "All Content")
                {
                    Console.WriteLine("All Content ,Ignore");
                    continue;
                }
                var subdocument = BrowsingContext.New(config).OpenAsync((item as IHtmlAnchorElement).Href).Result;
                var Titles = subdocument.QuerySelectorAll("div#content div h2");

                foreach (var title in Titles)
                {
                    TransAsset asset = new TransAsset();
                    asset.Title = title.FirstChild.Text();
                    asset.Href = (title.Children[0]as IHtmlAnchorElement).Href;
                    var metaElement = title.NextElementSibling;
                    while (metaElement != null && metaElement.TagName.ToLower() != "h2")
                    {
                        if(metaElement.ClassList.Contains("credits"))
                        {
                            asset.Credits = metaElement.TextContent;
                        }
                        if(metaElement.ClassList.Contains("abstract"))
                        {
                            asset.Abstract = metaElement.TextContent;
                        }
                        if(metaElement.ClassList.Contains("date"))
                        {
                            asset.Date = metaElement.TextContent;
                        }
                        if(metaElement.ClassList.Contains("type"))
                        {
                            asset.Type = metaElement.TextContent;
                        }                        
                        metaElement = metaElement.NextElementSibling;
                    }
                    
                    
//                    var abstrct = title.NextElementSibling;
//                    asset.Abstract = abstrct.TextContent;
//                    var date = abstrct.NextElementSibling;
//                    asset.Date = date.TextContent;
//                    var type = date.NextElementSibling;
//                    asset.Type = type.TextContent;

                    if (!_canTransTypes.Contains(asset.Type))
                    {
                        Console.WriteLine("Asset type not support:{0} - {1}",asset.Type,asset.Title);
                        continue;
                    }

                    if (_Contents.ContainsKey(asset.Href))
                    {
                        _Contents[asset.Href].Tags.Add(item.TextContent);
                    }
                    else
                    {
                        _Contents.Add(asset.Href,asset);
                        _Contents[asset.Href].Tags.Add(item.TextContent);
                    }
                }
            }

            File.WriteAllText(contentsJsonFileName,JsonConvert.SerializeObject(_Contents));
            
            Console.WriteLine("Summary: item count {0}",_Contents.Count);
        }

        public void CreateIssue()
        {
            if (File.Exists(contentsCreatedJsonFileName))
            {
                _ContentsCreated = JsonConvert.DeserializeObject<Dictionary<string, TransAsset>>(File.ReadAllText(contentsCreatedJsonFileName));
                Console.WriteLine("[WARN]ContentsCreated.json Exist, Will pass any issue in that list.");
                return;
            }

            int icount = 0;
            foreach (var asset in _Contents)
            {
                icount++;
                if (icount <= 300)
                {
                    Console.WriteLine("[Bypass] - ",asset.Value.Title);
                    continue;
                }
                
                if (!_ContentsCreated.ContainsKey(asset.Key) &&
                    !GitHubServices.Instance.IsIssueExist(asset.Value.Title, asset.Value.Href))
                {
                    var newIssue = new NewIssue(asset.Value.Title)
                    {
                        Body =
                            $"{asset.Value.Href}\n{asset.Value.Title}\n{asset.Value.Credits}\n{asset.Value.Date}\n{asset.Value.Abstract}"
                    };
                    foreach (var tag in asset.Value.Tags)
                    {
                        newIssue.Labels.Add(tag);
                    }

                    if (!string.IsNullOrEmpty(asset.Value.Type))
                    {
                        newIssue.Labels.Add(asset.Value.Type);
                    }
                    newIssue.Labels.Add(CONSTS.Label.Label_Welcome);
                    var createdIssue = GitHubServices.Instance.CreateIssue(newIssue);
                    _ContentsCreated.Add(asset.Key,asset.Value);
                    Console.WriteLine("Issue Created: #{0}",createdIssue.Number);
                }
                else
                {
                    if (!_ContentsCreated.ContainsKey(asset.Key))
                    {
                        _ContentsCreated.Add(asset.Key,asset.Value);
                    }
                    Console.WriteLine("Issue already exists: {0},{1}",asset.Value.Title,asset.Value.Href);
                }

                GitHubServices.Instance.CheckAPIRateLimit();
            }
            
            File.WriteAllText(contentsCreatedJsonFileName,JsonConvert.SerializeObject(_ContentsCreated));
        }


    }
}