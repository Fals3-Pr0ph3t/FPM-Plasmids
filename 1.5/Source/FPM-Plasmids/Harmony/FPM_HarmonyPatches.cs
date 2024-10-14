using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace FPMPlasmids
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            // Initialize and apply the Harmony patches
            var harmony = new Harmony("com.yourmod.patch");
            harmony.PatchAll();  // This will apply all Harmony patches in the assembly
        }
    }

    [HarmonyPatch(typeof(Gene), "get_Active")]
    public static class Gene_Active_Patch
    {
        public static void Postfix(Gene __instance, ref bool __result)
        {
            PlasmidFlag? plasmidFlag = FPM_Utils.GetPlasmidFlag(__instance);

            if (plasmidFlag == PlasmidFlag.Supressed)
            {
                __result = false;
            }
        }
    }
}