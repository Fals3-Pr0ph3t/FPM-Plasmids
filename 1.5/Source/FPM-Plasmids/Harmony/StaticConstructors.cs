using System.Text;
using Verse;
using HarmonyLib;
using RimWorld;

namespace FPMPlasmids
{
    [StaticConstructorOnStartup]
    public static class PlasmidDescriptionAmmender
    {
        /// Static constructor that amends the description of GeneDefs that are associated with plasmids.
        /// This modifies the gene descriptions to include information about the genes that the plasmid adds or removes.
        static PlasmidDescriptionAmmender() => LongEventHandler.ExecuteWhenFinished(() =>
        {
            // Iterate through all gene definitions in the database
            foreach (GeneDef geneDef in DefDatabase<GeneDef>.AllDefs)
            {
                // Get the custom mod extension for the gene, which contains plasmid data
                FPM_GeneExtension extension = geneDef.GetModExtension<FPM_GeneExtension>();

                // If the gene has an associated plasmid, modify its description
                if (extension?.plasmidInfo != null)
                {
                    // Start with the original gene description
                    StringBuilder sb = new StringBuilder(geneDef.description);

                    // If the plasmid adds genes, append that information to the description
                    if (extension.plasmidInfo.addedGenes != null && extension.plasmidInfo.addedGenes.Count != 0)
                    {
                        sb.AppendLine();
                        sb.AppendLine("This plasmid can add the following genes:");
                        foreach (GeneDef a in extension.plasmidInfo.addedGenes)
                        {
                            sb.AppendLine(a.label);
                        }
                    }

                    // If the plasmid removes genes, append that information to the description
                    if (extension.plasmidInfo.suppressedGenes != null && extension.plasmidInfo.suppressedGenes.Count != 0)
                    {
                        if (sb.Length > 0)
                        {
                            sb.AppendLine("\n" + "and will suppress the following genes:");
                        }
                        else
                        {
                            sb.AppendLine("This plasmid suppresses the following genes:");
                        }

                        foreach (GeneDef a in extension.plasmidInfo.suppressedGenes)
                        {
                            sb.AppendLine(a.label);
                        }
                    }

                    // Update the gene description with the amended text
                    geneDef.description = sb.ToString();
                }
            }
        });
    }

    public static class GameComponentInitializer
    {
        static GameComponentInitializer()
        {
            Log.Message("Registering PlasmidGameComponent");
            var harmony = new Harmony("FPM.Plasmids.patch");
            harmony.PatchAll();
        }
    }

    [DefOf]
    public static class HediffDefOf
    {
        public static HediffDef PlasmidFlu;
        public static HediffDef PlasmidRegrow;

        static HediffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
        }
    }
}