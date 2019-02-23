

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sleepovers
{
    public class BedLoc
    {
        public string MapName;
        public int MapX;
        public int MapY;
        public BedLoc() { }
        public BedLoc(string mn, int x, int y)
        {
            MapName = mn;
            MapX = x;
            MapY = y;
        }

        public BedLoc(string str)
        {
            string[] ss = str.Split('.');
            MapName = ss[0];
            MapX = int.Parse(ss[1]);
            MapY = int.Parse(ss[2]);
        }

        public string str()
        {
            return MapName + "." + MapX + "." + MapY;
        }
    }

    class Config
    {
        public static Dictionary<string, string> Beds; //key npc, value bedloc
        public static bool ready = false;
        public static void Load()
        {
            try
            {
                string filecontents = File.ReadAllText(Mod.Instance.Helper.DirectoryPath + Path.DirectorySeparatorChar + "beds.json");
                Beds = JsonConvert.DeserializeObject<Dictionary<string, string>>(filecontents);
                ready = true;
            }
            catch (Exception e)
            {
                Mod.Instance.Monitor.Log("Failed to read bed config file: " + e.Message, StardewModdingAPI.LogLevel.Error);
            }
        }

        public static BedLoc GetBedLocation(string npc)
        {
            if (Beds.ContainsKey(npc)) return new BedLoc(Beds[npc]);
            return null;
        }

        public static string GetNPC(BedLoc bedLocation)
        {
            foreach (var b in Beds)
            {
                if (b.Value == bedLocation.str()) return b.Key;
            }
            return null;
        }
    }
}
