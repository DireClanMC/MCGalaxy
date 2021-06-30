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
    public class CmdLocation2 : Command2
    {
        public override string name { get { return "Location2"; } }
        public override string shortcut { get { return "loc2"; } }
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
                WebRequest req = HttpUtil.CreateRequest("http://ipinfo.io/" + ip + "/geo");
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

            object region = null, country = null, city = null, hostname = null;
            obj.TryGetValue("region", out region);
            obj.TryGetValue("country", out country);
            obj.TryGetValue("city", out city);
            obj.TryGetValue("hostname", out hostname);

            string target = name == null ? ip : "of " + p.FormatNick(name);
            p.Message("The IP {0} %Shas been traced to: ", target);
            p.Message("  Country: &f{0}", country);
            p.Message("  Region/State: &f{0}", region);
            p.Message("  City: &f{0}", city);
            p.Message("  Hostname: &f{0}", hostname);
            p.Message("Geoip information by: &9http://ipinfo.io/");
        }

        public override void Help(Player p)
        {
            p.Message("%T/GeoIP [name/IP]");
            p.Message("%HProvides detailed output on a player or an IP a player is on.");
        }
    }
}

