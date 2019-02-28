using StardewModdingAPI;
using StardewValley;
using StardewModdingAPI.Events;
using System.Collections.Generic;
using System;
using Modworks = bwdyworks.Modworks;
using Microsoft.Xna.Framework;
using System.IO;
using Newtonsoft.Json;

namespace Sleepovers
{
    public class Mod : StardewModdingAPI.Mod
    {
        internal static bool Debug = false;
        [System.Diagnostics.Conditional("DEBUG")]
        public void EntryDebug() { Debug = true; }
        internal static string Module;

        public static List<string> NoSleep;
        public static List<string> Attempts = new List<string>();

        public override void Entry(IModHelper helper)
        {
            Module = helper.ModRegistry.ModID;
            EntryDebug();
            if (!Modworks.InstallModule(Module, Debug)) return;

            Modworks.Events.NPCCheckAction += Events_NPCCheckAction;

            try
            {
                string filecontents = File.ReadAllText(Helper.DirectoryPath + Path.DirectorySeparatorChar + "config.json");
                NoSleep = JsonConvert.DeserializeObject<List<string>>(filecontents);
            }
            catch (Exception)
            {
                NoSleep = new List<string>();
            }
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
        }

        private void Events_NPCCheckAction(object sender, bwdyworks.Events.NPCCheckActionEventArgs args)
        {
            if (args.Cancelled) return; //someone else already ate this one
            if (Game1.player.ActiveObject == null) //empty hands to sleep
            {
                if (Attempts.Contains(args.NPC.Name)) return; //already tried. no means no.

                //check if NPC is present
                if (IsNPCInBed(args.NPC))
                {
                    args.Cancelled = true;
                    Modworks.Menus.AskQuestion("Sleepover with " + args.NPC.Name + "?", new[] { new Response(args.NPC.Name, "Yes"), new Response(".nope.", "No") }, QuestionCallback);
                }
                return;
            }
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            Attempts.Clear();
        }

        public Point? GetBedLocation(NPC c)
        {
            int latest = 0;
            if (c == null) return null;
            if (c.Schedule == null) return null;
            foreach(var kvp in c.Schedule)
            {
                if (kvp.Key > latest) latest = kvp.Key;
            }
            if (c.Schedule.ContainsKey(latest))
            {
                Point[] paths = new Point[c.Schedule[latest].route.Count];
                c.Schedule[latest].route.CopyTo(paths, 0);
                return paths[paths.Length - 1];
            }
            return null;
        }

        public bool IsNPCInBed(NPC npc)
        {
            if (npc.currentLocation != npc.getHome()) return false;
            Point? bedPoint = GetBedLocation(npc);
            if (!bedPoint.HasValue) return false;
            if (npc.getTileX() != bedPoint.Value.X) return false;
            if (npc.getTileY() != bedPoint.Value.Y) return false;
            return true;
        }

        public void DoSleepover()
        {
            Game1.player.isInBed.Value = true;
            Game1.NewDay(1f);
        }

        public void QuestionCallback(Farmer who, string npc)
        {
            if (npc != ".nope.")
            {
                Attempts.Add(npc);

                int friendship = Modworks.Player.GetFriendshipPoints(npc);
                if(friendship < 500)
                {
                    //offensive to even ask - you shouldn't be in the room.
                    Game1.showRedMessage(npc + " is offended you would ask.");
                    Modworks.Player.SetFriendshipPoints(npc, Math.Max(0, friendship - 50));
                } else if(friendship < 750)
                {
                    Game1.showRedMessage(npc + " doesn't know you that well.");
                } else
                {
                    float chances = (float)friendship / 2600f; //1000 would be just shy of 0.4
                    Random rng = new Random(DateTime.Now.Millisecond);
                    if (rng.NextDouble() <= chances)
                    {
                        Modworks.Player.SetFriendshipPoints(npc, Math.Min(2500, friendship + 50));
                        DoSleepover();
                    } else
                    {
                        Game1.showRedMessage(npc + " doesn't want to.");
                    }
                }

            }
        }
    }
}