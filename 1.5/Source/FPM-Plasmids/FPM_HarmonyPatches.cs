using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace FPMPlasmids
{
    [HarmonyPatch(typeof(Pawn), "Tick")]
    public static class Patch_Pawn_Tick
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn __instance)
        {
            if (__instance.IsHashIntervalTick(60000))
            {
                var activeExtensions = __instance.genes.GetActiveGeneExtensions();
                if (CheckRepair(activeExtensions))
                {
                    FPM_GeneExtension.GeneRepair(__instance);
                }
            }
        }

        public static bool CheckRepair(List<FPM_GeneExtension> extensions)
        {
            return extensions.Any(extension => extension.geneRepair);
        }
    }

    [HarmonyPatch(typeof(Pawn_GeneTracker), "Notify_GenesChanged")]
    public static class Patch_Notify_GenesChanged
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn_GeneTracker __instance, GeneDef addedOrRemovedGene)
        {
            FPM_GeneExtension.PlasmidImplanter(__instance.pawn);
        }
    }
}