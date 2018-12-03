using Octokit;

namespace IssueHookAPI
{
    public class CONSTS
    {
        public class Label
        {
            public static string Label_Welcome = "Welcome";
            public static string Label_Translating = "Translating";
            public static string Label_Pushed = "Pushed";
            public static string Label_Finished = "Finished";
        }
        public class Command
        {
            public static string Cmd_Accept = "/Accept";
            public static string Cmd_Pushed = "/Pushed";
            public static string Cmd_Merged = "/Merged";
        }        
    }
}