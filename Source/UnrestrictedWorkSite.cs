using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Jetroid.UnrestrictedWorkSite
{
    // This class runs when the mod is loaded and applies all Harmony patches
    public class UnrestrictedWorkSiteMod : Mod
    {
        public UnrestrictedWorkSiteMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("Jetroid.UnrestrictedWorkSite");
            harmony.PatchAll();
        }
    }
    [HarmonyPatch(typeof(RimWorld.QuestGen.QuestNode_Root_WorkSite))]
    [HarmonyPatch("AppearanceFrequency")]
    public static class Patch_QuestNode_Root_WorkSite_AppearanceFrequency
    {
        public static bool Prefix(Map map, ref float __result)
        {
            float num = 1f;
            float num2 = 0f;
            List<PlanetTile> list = RimWorld.QuestGen.QuestNode_Root_WorkSite.PotentialSiteTiles(map.Tile);
            if (list.Count == 0)
            {
                __result = 0f;
                return false;
            }

            var anySpawnCandidateMethod = AccessTools.Method(typeof(RimWorld.QuestGen.QuestNode_Root_WorkSite), "AnySpawnCandidate");
            if (!(bool)anySpawnCandidateMethod.Invoke(null, new object[] { map.Tile }))
            {
                __result = 0f;
                return false;
            }

            foreach (PlanetTile item in list)
            {
                num2 += Find.WorldGrid[item].PrimaryBiome.campSelectionWeight;
            }

            num2 /= (float)list.Count;
            num *= num2;

            int num3 = 0;
            foreach (Site site in Find.WorldObjects.Sites)
            {
                if (site.MainSitePartDef.tags != null && site.MainSitePartDef.tags.Contains("WorkSite"))
                {
                    num3++;
                }
            }

            var multiplierField = AccessTools.Field(typeof(RimWorld.QuestGen.QuestNode_Root_WorkSite), "ExistingCampsAppearanceFrequencyMultiplier");
            SimpleCurve multiplier = (SimpleCurve)multiplierField.GetValue(null);
            num *= multiplier.Evaluate(num3);

            int num4 = map.mapPawns.FreeColonists.Count();

            // THIS IS THE PART WE REPLACE -
            // THE NORMAL LOGIC MAKES WORKSITES NOT GENERATE FOR SINGLE PAWNS
            // OR HALVES FREQUENCY FOR TWO PAWNS
            // WE CHANGE THIS TO ONLY NOT GENERATE FOR ZERO PAWNS
            if (num4 == 0)
            {
                __result = 0f;
                return false;
            }
            __result = num;
            return false; // skip original method entirely
        }
    }
}