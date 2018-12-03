using System.Collections.Generic;

namespace TransTaskIssueGenerator
{
    public class TransAsset
    {
        public string Title { get; set; }
        public string Href { get; set; }
        public string Credits { get; set; }
        public List<string> Tags { get; set; }
        public string Type { get; set; }
        public string Date { get; set; }
        public string Abstract { get; set; }

        public TransAsset()
        {
            Tags = new List<string>();
        }
    }
}