﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Valour.Server.MSP
{
    public static class MSPManager
    {
        public static HttpClient Http = new HttpClient();

        public const string MSP_PROXY = "https://msp.valour.gg/u/proxy";

        public static async Task<MSPResponse> GetProxy(string url)
        {
            string encoded_url = HttpUtility.UrlEncode(url);

            var response = await Http.GetAsync($"{MSP_PROXY}?auth={MSPConfig.Current.Api_Key}&url={encoded_url}");

            MSPResponse data = JsonConvert.DeserializeObject<MSPResponse>(await response.Content.ReadAsStringAsync());

            return data;
        }

        public static Regex Url_Regex = new Regex(@"(http|https|)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([=a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_]*)?([=a-zA-Z0-9\-\?\,\'\/\+&%\$#_]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static List<string> Media_Bypass = new List<string>()
        {
            "https://youtube.com/watch",
            "https://cdn.discordapp.com/",
            "https://vimeo.com/",
            "https://tenor.com/",
            "https://i.imgur.com/",
            "https://youtu.be/",
            "https://msp.valour.gg",
            "https://valour.gg",
            "https://pbs.twimg.com",

            "http://youtube.com/watch",
            "http://cdn.discordapp.com/",
            "http://vimeo.com/",
            "http://tenor.com/",
            "http://i.imgur.com/",
            "http://youtu.be/",
            "http://msp.valour.gg",
            "http://valour.gg",
            "http://pbs.twimg.com",
        };

        public static async Task<string> HandleUrls(string content)
        {
            MatchCollection matches = Url_Regex.Matches(content);

            foreach (Match match in matches)
            {
                bool bypass = false;

                foreach (string s in Media_Bypass)
                {
                    if (match.Value.ToLower().Replace("www.", "").StartsWith(s))
                    {
                        bypass = true;
                        content = content.Replace(match.Value, $"![]({match.Value})");
                        break;
                    }
                }

                if (!bypass) {
                    MSPResponse proxied = await GetProxy(match.Value);

                    if (proxied.Is_Media)
                    {
                        content = content.Replace(match.Value, $"![]({proxied.Url})");
                    }
                }
            }

            return content;
        }
    }
}
