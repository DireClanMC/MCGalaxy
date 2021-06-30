/*
    Copyright 2015 MCGalaxy team

    Dual-licensed under the    Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    http://www.opensource.org/licenses/ecl2.php
    http://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using System;
using System.Net;
using MCGalaxy.Config;
using MCGalaxy.Network;

namespace MCGalaxy.Commands.Moderation {
    public class CmdLocation3 : Command2
    {
        public override string name { get { return "Location3"; } }
        public override string shortcut { get { return "loc3"; } }
        public override string type { get { return CommandTypes.Moderation; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }

        public override void Use(Player p, string message, CommandData data)
        {
            if (message.Length == 0)
            {
                if (p.IsSuper) { SuperRequiresArgs(p, "player name or IP"); return; }
                message = p.name;
            }

            string name, ip = ModActionCmd.FindIP(p, message, "Location", out name);
            if (ip == null) return;

            if (IPUtil.IsPrivate(IPAddress.Parse(ip)))
            {
                p.Message("%WPlayer has an internal IP, cannot trace"); return;
            }

            string json;
            try
            {
                WebRequest req = HttpUtil.CreateRequest("https://geoip.pw/api/" + ip);
                WebResponse res = req.GetResponse();
                json = HttpUtil.GetResponseText(res);
            }
            catch (Exception ex)
            {
                HttpUtil.DisposeErrorResponse(ex);
                throw;
            }

            JsonReader reader = new JsonReader(json);
            JsonObject obj = (JsonObject)reader.Parse();
            if (obj == null) { p.Message("&WError parsing GeoIP info"); return; }

            object summary = null, IP = null, continent = null, region = null, country = null, city = null, subdivision = null, continent_abbr = null, country_abbr = null, host = null, timezone = null, proxy = null;
            obj.TryGetValue("summary", out summary);
            obj.TryGetValue("ip", out IP);
            obj.TryGetValue("continent", out continent);
            obj.TryGetValue("continent_abbr", out continent_abbr);
            obj.TryGetValue("region", out region);
            obj.TryGetValue("country", out country);
            obj.TryGetValue("country_abbr", out country_abbr);
            obj.TryGetValue("city", out city);
            obj.TryGetValue("host", out host);
            obj.TryGetValue("proxy", out proxy);
            obj.TryGetValue("subdivision", out subdivision);
            obj.TryGetValue("timezone", out timezone);

            string target = name == null ? ip : "of " + p.FormatNick(name);
            p.Message("The IP {0} %Shas been traced to: ", target);
            p.Message("  IP Address: &f{0}&S", IP);
            p.Message("  Summary: &f{0}&S", summary);
            p.Message("  Continent: &f{1}&S ({0})", continent_abbr, continent);
            p.Message("  Country: &f{1}&S ({0})", country_abbr, country);
            p.Message("  Region/State: &f{0}", subdivision);
            p.Message("  City: &f{0}", city);
            p.Message("  Time Zone: &f{0}", timezone);
            p.Message("  Hostname: &f{0}", host);
            p.Message("  Is using proxy: &f{0}", proxy);
            p.Message("Geoip information by: &9http://geoip.pw/");
        }

        public override void Help(Player p)
        {
            p.Message("%T/GeoIP [name/IP]");
            p.Message("%HProvides detailed output on a player or an IP a player is on.");
        }
    }
}