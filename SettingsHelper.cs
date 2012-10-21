using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace WoWApiHelper
{
    public static class SettingsHelper
    {
        public static ApiAccessSettings GetSettings()
        {
            ApiAccessSettings settings = new ApiAccessSettings();

            settings.Region = ConfigurationSettings.AppSettings["serverRegion"].ToRegion();

            string guildString = ConfigurationSettings.AppSettings["guilds"];
            
            foreach (string guild in guildString.Split(",".ToCharArray()))
            {
                string[] args = guild.Split("|".ToCharArray());
                settings.Guilds.Add(args[0], args[1]);
            }

            return settings;
        }

        private static RegionsLookup ToRegion(this string regionText)
        {
            RegionsLookup region = RegionsLookup.EU;
            switch(regionText)
            {
                case "eu":
                    region = RegionsLookup.EU;
                    break;
                case "us":
                    region = RegionsLookup.US;
                    break;
            }
            return region;
        }
    }

    public class ApiAccessSettings
    {
        public ApiAccessSettings()
        {
            Guilds = new Dictionary<string,string>();
        }

        public Dictionary<string, string> Guilds { get; set; }
        public RegionsLookup Region { get; set; }
    }
}
