using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using AngleSharp;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;

namespace TransTaskIssueGenerator
{
    public class ContentIndexService
    {
        public static readonly ContentIndexService Instance = new ContentIndexService();
        private Dictionary<string, TransAsset> _Contents = new Dictionary<string, TransAsset>();
        private string[] _canTransTypes = new[] {"article", "bliki",null};
        private ContentIndexService()
        {
        }

        public void BuildIndex()
        {
            var config = Configuration.Default.WithDefaultLoader();
            var document =  BrowsingContext.New(config).OpenAsync("https://martinfowler.com/tags/").Result;            
           
            var tags = document.QuerySelectorAll("div.tags p a");
            foreach (var item in tags)
            {
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
                    while (metaElement != null)
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
            
            Console.WriteLine("Summary: item count {0}",_Contents.Count);
        }

        public void CreateIssue()
        {
            
        }
    }
}