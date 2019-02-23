using StardewModdingAPI;
using StardewValley;
using StardewModdingAPI.Events;
using System.Collections.Generic;
using System;

namespace Sleepovers
{
    public class Mod : StardewModdingAPI.Mod
    {
#if DEBUG
        private static readonly bool DEBUG = true;
#else
        private static readonly bool DEBUG = false;
#endif
        public static Mod Instance;
        public static bwdyworks.ModUtil ModUtil;
        public static List<string> Attempts = new List<string>();

        public override void Entry(IModHelper helper)
        {
            ModUtil = new bwdyworks.ModUtil(this);
            Instance = this;
            if(ModUtil.StartConfig(DEBUG))
            {
                Config.Load();
                if (!Config.ready) return;
                helper.Events.Input.ButtonPressed += Input_ButtonPressed;
                helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
                ModUtil.EndConfig();
            }
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            Attempts.Clear();
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button.IsActionButton())
            {
                if (Context.IsPlayerFree)
                {
                    var target = ModUtil.GetLocalPlayerFacingTileCoordinate();
                    var key = Game1.currentLocation.Name + "." + target[0] + "." + target[1];
                    //check out bed in front of player
                    var bed = Config.GetNPC(new BedLoc(key));
                    if (bed != null)
                    {
                        if (Attempts.Contains(bed)) return; //already tried. no means no.
                        StardewValley.NPC npci = Game1.getCharacterFromName(bed);
                        
                        //check if NPC is present
                        if(IsNPCInBed(Game1.getCharacterFromName(bed), new BedLoc(key)))
                        {
                            Helper.Input.Suppress(e.Button);
                            ModUtil.AskQuestion("Sleepover with " + bed + "?", new[] { new Response(bed, "Yes"), new Response(".nope.", "No") }, QuestionCallback);
                        }
                        return;
                    }

                    target = ModUtil.GetLocalPlayerStandingTileCoordinate();
                    key = Game1.currentLocation.Name + "." + target[0] + "." + target[1];

                    //check out bed under player (woohoo you're already on top of them)
                    bed = Config.GetNPC(new BedLoc(key));
                    if (bed != null)
                    {
                        if (Attempts.Contains(bed)) return; //already tried. no means no.
                        StardewValley.NPC npci = Game1.getCharacterFromName(bed);

                        //check if NPC is present
                        if (IsNPCInBed(Game1.getCharacterFromName(bed), new BedLoc(key)))
                        {
                            //check if NPC is present
                            Helper.Input.Suppress(e.Button);
                            ModUtil.AskQuestion("Sleepover with " + bed + "?", new[] { new Response(bed, "Yes"), new Response(".nope.", "No") }, QuestionCallback);
                        }
                        return;
                    }
                }
            }
        }

        public bool IsNPCInBed(StardewValley.NPC npc, BedLoc bed)
        {
            if (npc.getTileX() != bed.MapX) return false;
            if (npc.getTileY() != bed.MapY) return false;
            if (npc.currentLocation.Name != bed.MapName) return false;
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

                int friendship = ModUtil.GetFriendshipPoints(npc);
                if(friendship < 500)
                {
                    //offensive to even ask - you shouldn't be in the room.
                    Game1.showRedMessage(npc + " is offended you would ask.");
                    ModUtil.SetFriendshipPoints(npc, Math.Max(0, friendship - 50));
                } else if(friendship < 750)
                {
                    Game1.showRedMessage(npc + " doesn't know you that well.");
                } else
                {
                    float chances = (float)friendship / 2600f; //1000 would be just shy of 0.4
                    Random rng = new Random(DateTime.Now.Millisecond);
                    if (rng.NextDouble() <= chances)
                    {
                        ModUtil.SetFriendshipPoints(npc, Math.Min(2500, friendship + 50));
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