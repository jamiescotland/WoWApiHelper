using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Runtime.Caching;
using System.Web.Script.Serialization;
using System.Net;
using System.IO;
using System.Configuration;

namespace WoWApiHelper
{
    public static class WowApiHelper
    {
        public static IEnumerable<CharacterModel> GetGuildMembers()
        {
            ApiAccessSettings settings = SettingsHelper.GetSettings();
            
            ObjectCache cache = MemoryCache.Default;

            List<CharacterModel> characters = cache["guildMembers"] as List<CharacterModel>;

            if (characters == null)
            {
                characters = new List<CharacterModel>();

                foreach (KeyValuePair<string, string> kvp in settings.Guilds)
                {
                    characters.AddRange(GetGuildMembers(settings.Region, kvp.Key, kvp.Value));
                }
                
                cache.Set("guildMembers", characters, DateTimeOffset.Now.AddHours(1));

            }
            return characters;
        }

        private static IEnumerable<CharacterModel> GetGuildMembers(RegionsLookup region, string realm, string guildName)
        {
            List<CharacterModel> characters = new List<CharacterModel>();

            dynamic guild = GetGuildInfo(region, realm, guildName);

            foreach (dynamic character in guild["members"])
            {
                try
                {
                    string name = character["character"]["name"];

                    CharacterModel c = GetCharacter(region, realm, name);
                    if (c != null)
                    {
                        characters.Add(c);
                    }
                }
                finally { }
            }

            return characters;
        }

        public static IEnumerable<dynamic> GetGuildMembersDetails()
        {
            ApiAccessSettings settings = SettingsHelper.GetSettings();

            ObjectCache cache = MemoryCache.Default;

            List<dynamic> characters = cache["guildMembers"] as List<dynamic>;

            if (characters == null)
            {
                characters = new List<dynamic>();

                foreach (KeyValuePair<string, string> kvp in settings.Guilds)
                {
                    characters.AddRange(GetGuildMembersDetails(settings.Region, kvp.Key, kvp.Value));
                }

                cache.Set("guildMembers", characters, DateTimeOffset.Now.AddHours(1));

            }
            return characters;
        }

        private static IEnumerable<dynamic> GetGuildMembersDetails(RegionsLookup region, string realm, string guildName)
        {
            List<dynamic> characters = new List<dynamic>();

            dynamic guild = GetGuildInfo(region, realm, guildName);

            foreach (dynamic character in guild["members"])
            {
                try
                {
                    string name = character["character"]["name"];

                    dynamic c = GetCharacterDetail(region, realm, name);
                    if (c != null)
                    {
                        characters.Add(c);
                    }
                }
                catch { }
            }

            return characters;
        }

        public static IEnumerable<dynamic> GetGuildInfo()
        {
            ApiAccessSettings settings = SettingsHelper.GetSettings();

            List<dynamic> result = new List<dynamic>();

            foreach (KeyValuePair<string, string> kvp in settings.Guilds)
            {
                result.Add(GetGuildInfo(settings.Region,kvp.Key,kvp.Value));
            }

            return result;
        }

        private static dynamic GetGuildInfo(RegionsLookup region, string server, string guild)
        {
            string key =string.Format("guild_{0}_{1}_{2}", region, server, guild);

            ObjectCache cache = MemoryCache.Default;
            
            dynamic item = cache[key];

            if (item == null)
            {
                string requestUrl = string.Format("{0}/guild/{1}/{2}?fields=news,members,achievements,challenge", GetBaseRequestURL(region), server, guild);

                item = MakeRequest(requestUrl);
                                
                cache.Set(key, item, DateTimeOffset.Now.AddHours(1));
            }

            return item;
        }

        public static dynamic GetCharacterRaces()
        {
            ApiAccessSettings settings = SettingsHelper.GetSettings();

            ObjectCache cache = MemoryCache.Default;

            dynamic item = cache["characterraces"];

            if (item == null)
            {
                string requestUrl = string.Format("{0}/data/character/races", GetBaseRequestURL(settings.Region));
                item = MakeRequest(requestUrl);

                cache.Set("characterraces", item, DateTimeOffset.Now.AddDays(7));
            }

            return item;
        }

        public static dynamic GetCharacterClasses()
        {
            ApiAccessSettings settings = SettingsHelper.GetSettings();

            ObjectCache cache = MemoryCache.Default;

            dynamic item = cache["characterclasses"];

            if(item==null)
            {
                string requestUrl = string.Format("{0}/data/character/classes", GetBaseRequestURL(settings.Region));
                item = MakeRequest(requestUrl);

                cache.Set("characterclasses",item,DateTimeOffset.Now.AddDays(7));
            }

            return item;
        }

        public static dynamic GetCharacterAchievements()
        {
            ApiAccessSettings settings = SettingsHelper.GetSettings();

            ObjectCache cache = MemoryCache.Default;

            dynamic item = cache["characterachievements"];

            if (item == null)
            {
                string requestUrl = string.Format("{0}/data/character/achievements", GetBaseRequestURL(settings.Region));
                item = MakeRequest(requestUrl);

                cache.Set("characterachievements", item, DateTimeOffset.Now.AddDays(7));
            }
            return item;
        }

        public static dynamic GetGuildAchievements()
        {
            ApiAccessSettings settings = SettingsHelper.GetSettings();

            ObjectCache cache = MemoryCache.Default;

            dynamic item = cache["guildachievements"];

            if (item == null)
            {
                string requestUrl = string.Format("{0}/data/guild/achievements", GetBaseRequestURL(settings.Region));
                item = MakeRequest(requestUrl);

                cache.Set("guildachievements", item, DateTimeOffset.Now.AddDays(7));

            }
            return item;
        }

        private static dynamic MakeRequest(string requestUrl)
        {
            try
            {
                WebClient client = new WebClient();
                string result = client.DownloadString(requestUrl);

                JavaScriptSerializer jss = new JavaScriptSerializer();

                dynamic item = jss.Deserialize<dynamic>(result);

                return item;
            }
            catch (WebException webEx)
            {
                throw webEx;
            }
        }

        /// <summary>
        /// Get's the base url to the Blizzard API for the given region.
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        private static string GetBaseRequestURL(RegionsLookup region)
        {
            string r=string.Empty;
            switch (region)
            {
                case RegionsLookup.EU:
                    r = "eu";
                    break;
                case RegionsLookup.US:
                    r = "us";
                    break;
            }
            return string.Format("http://{0}.battle.net/api/wow", r);
        }

        public static dynamic GetCharacterDetail(RegionsLookup region, string server, string name)
        {
            string requestUrl = string.Format("{0}/character/{1}/{2}?fields=items", GetBaseRequestURL(region), server, name);

            return MakeRequest(requestUrl);
        }

        public static CharacterModel GetCharacter(RegionsLookup region, string server, string name)
        {
            string key = string.Format("character_{0}_{1}_{2}", region, server, name);

            ObjectCache cache = MemoryCache.Default;
            CharacterModel item = cache[key] as CharacterModel;

            if (item == null)
            {
                string requestUrl = string.Format("{0}/character/{1}/{2}", GetBaseRequestURL(region), server, name);

                try
                {
                    WebClient client = new WebClient();
                    string result = client.DownloadString(requestUrl);

                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CharacterModel));

                    using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(result)))
                    {
                        item = serializer.ReadObject(ms) as CharacterModel;
                    }

                    DateTimeOffset dto = new DateTimeOffset();

                    dto.AddHours(1);

                    cache.Set(key, item, DateTimeOffset.Now.AddMinutes(30));
                }
                catch (Exception ex)
                {
                }
            }

            return item;
        }

        public static List<GuildNews> GetGuildNews()
        {
            ApiAccessSettings settings = SettingsHelper.GetSettings();

            ObjectCache cache = MemoryCache.Default;


            List<GuildNews> guildNews = cache["guildNews"] as List<GuildNews>;

            if (guildNews == null)
            {
                guildNews = new List<GuildNews>();

                foreach (KeyValuePair<string, string> kvp in settings.Guilds)
                {
                    string requestUrl = string.Format("{0}/guild/{1}/{2}?fields=news", GetBaseRequestURL(settings.Region), kvp.Key, kvp.Value);

                    WebClient client = new WebClient();
                    string result = client.DownloadString(requestUrl);

                    JavaScriptSerializer jss = new JavaScriptSerializer();

                    var item = jss.Deserialize<dynamic>(result);

                    foreach (var newsItem in item["news"])
                    {
                        string middle = string.Empty;
                        ItemDetail itemDetail;
                        switch ((String)newsItem["type"])
                        {
                            case "itemPurchase":
                                itemDetail = GetItemDetail(newsItem["itemId"]);
                                middle = string.Format("{0} has bought <a href='http://www.wowhead.com/item={1}' rel='item={1}'>{2}</a>", newsItem["character"], itemDetail.ID, itemDetail.Name);
                                break;
                            case "itemLoot":
                                itemDetail = GetItemDetail(newsItem["itemId"]);
                                middle = string.Format("{0} has found <a href='http://www.wowhead.com/item={1}' rel='item={1}'>{2}</a>", newsItem["character"], itemDetail.ID, itemDetail.Name);
                                break;
                            case "itemCraft":
                                itemDetail = GetItemDetail(newsItem["itemId"]);
                                middle = string.Format("{0} has crafted <a href='http://www.wowhead.com/item={1}' rel='item={1}'>{2}</a>", newsItem["character"], itemDetail.ID, itemDetail.Name);
                                break;
                            case "playerAchievement":
                                middle = string.Format("{0} has earned the achievement <a href='http://www.wowhead.com/achievement={1}' rel='achievement={1}'>{2}</a>", newsItem["character"], newsItem["achievement"]["id"], newsItem["achievement"]["title"]);
                                break;
                            case "guildAchievement":
                                middle = string.Format("The guild has earned the achievement <a href='http://www.wowhead.com/achievement={1}' rel='achievement={1}'>{2}</a>", newsItem["character"], newsItem["achievement"]["id"], newsItem["achievement"]["title"]);
                                break;
                            case "guildLevel":
                                middle = string.Format("The guild has reached level {0}", newsItem["levelUp"]);
                                break;
                        }
                        guildNews.Add(new GuildNews() { Text = middle });
                    }
                }
                //Add to cache
                cache.Set("guildNews", guildNews, DateTimeOffset.Now.AddMinutes(30));
            }
            return guildNews;
        }

        public static ItemDetail GetItemDetail(int ID)
        {
            string requestUrl = string.Format("http://eu.battle.net/api/wow/item/{0}", ID);

            WebClient client = new WebClient();
            string result = client.DownloadString(requestUrl);

            JavaScriptSerializer jss = new JavaScriptSerializer();

            var item = jss.Deserialize<dynamic>(result);

            ItemDetail itemDetail = new ItemDetail();
            itemDetail.ID = ID;
            itemDetail.Name = (string)item["name"];

            return itemDetail;
        }

    }

    [DataContract()]
    public class GuildNews : IExtensibleDataObject
    {
        [DataMember(Name = "text")]
        public string Text { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract()]
    public class ItemDetail : IExtensibleDataObject
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "id")]
        public int ID { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }
}
