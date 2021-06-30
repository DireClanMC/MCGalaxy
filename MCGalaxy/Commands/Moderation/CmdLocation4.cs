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

namespace MCGalaxy.Commands.Moderation
{
    public class CmdLocation4 : Command2 {
        public override string name { get { return "Location4"; } }
        public override string shortcut { get { return "loc4"; } }
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
                WebRequest req = HttpUtil.CreateRequest("http://api.db-ip.com/v2/free/" + ip);
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

            object continentname = null, continentcode = null, countryname, countrycode = null, stateprov, city = null, eumember = null, currencycode = null, currencyname = null;
            obj.TryGetValue("continentName", out continentname);
            obj.TryGetValue("continentCode", out continentcode);
            obj.TryGetValue("countryName", out countryname);
            obj.TryGetValue("countryCode", out countrycode);
            obj.TryGetValue("currencyCode", out currencycode);
            obj.TryGetValue("currencyName", out currencyname);
            obj.TryGetValue("isEuMember", out eumember);


            string target = name == null ? ip : "of " + p.FormatNick(name);
            p.Message("The IP {0} %Shas been traced to: ", target);
            p.Message("  Continent: &f{0}&S ({1})", continentname, continentcode);
            p.Message("  Country: &f{0}&S ({1})", countryname, countrycode);
            p.Message("  EU Member: &f{0}", eumember);
            p.Message("  Currency: &f{0}&S ({1})", currencyname, currencycode);
            p.Message("  EU Member: &f{0}", eumember);

            p.Message("Geoip information by: &9https://db-ip.com");
        }

        public override void Help(Player p)
        {
            p.Message("%T/GeoIP [name/IP]");
            p.Message("%HProvides detailed output on a player or an IP a player is on.");
        }
    }
}
