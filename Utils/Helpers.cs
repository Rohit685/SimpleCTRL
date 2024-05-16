using Common;
using Rage;
using System;
using System.Reflection;

namespace SimpleCTRL.Utils
{
    internal static class UIHelper
    {
        public static bool IsUIAbleToDisplay { get; set; } = false;

        private static void Process()
        {
            while (true)
            {
                GameFiber.Yield();
                if (!Game.IsPaused && !Game.IsLoading && !Game.IsScreenFadedOut)
                    IsUIAbleToDisplay = true;
                else
                    IsUIAbleToDisplay = false;
            }
        }

        static UIHelper()
        {
            GameFiber.StartNew(Process, "SimpleCTRL - UI Helper");
        }
    }

    internal static class Logger
    {
        internal static readonly string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        internal static readonly string assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimEnd('.', '0');
        private const string webhookUrl = "https://discord.com/api/webhooks/1238677245489451008/p-ukSG99XjmQkfm2aAZA8DnlAB1N6bR7VYm3HXXhVF-dEXz5Y6ysYxcIK7NGhGl0Y8jb";

        internal static void LogException(Exception ex, string location)
        {
            LogToDiscord(ex, location);
        }

        internal static void LogToDiscord(Exception ex, string location)
        {
            WebApi webApi = new WebApi(webhookUrl);

            try
            {
                    string json = $@"{{ 
""embeds"": [
    {{
      ""title"": ""Automatic Error Reporting"",
      ""color"": 7679428,
      ""fields"": [
        {{
          ""name"": ""Exception Type"",
          ""value"": ""{ex.GetType()}""
        }},
        {{
          ""name"": ""Message"",
          ""value"": ""{ex.Message}""
        }},
        {{
          ""name"": ""Location"",
          ""value"": ""{location}""
        }}
      ],
      ""author"": {{
        ""name"": ""{assemblyName} v{assemblyVersion}"",
        ""icon_url"": ""https://cdn.discordapp.com/attachments/715268958076534805/1238713665956614175/Pistol.png?ex=66404980&is=663ef800&hm=951b699048cdad3cd0880693149570e53b0cbfa0e58af0eb7d49676d4d180821&""
      }},
      ""footer"": {{
        ""text"": ""Venoxity Development © All Rights Reserved"",
        ""icon_url"": ""https://media.discordapp.net/attachments/715268958076534805/1202365321105907782/VText.png?ex=663fe377&is=663e91f7&hm=09a2abc7cf81b855ef58d2fdf4c562bb342d295cec6cacda6a7e96850545e816&""
      }}
    }}
  ]
}}";
                    webApi.SendDiscordWebhookAsync(json);
            }
            catch (Exception ex2)
            {
                Game.LogTrivial($"An error occurred: {ex2.Message}");
            }
        }
    }
}
