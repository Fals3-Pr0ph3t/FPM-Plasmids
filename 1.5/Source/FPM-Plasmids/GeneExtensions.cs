using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace FPMPlasmids
{
    public class FPM_GeneExtension : DefModExtension
    {
        public bool geneRepair = false;
        public bool plasmidImplanter = false;

        public int IntPerGene = 60000;

        public static void PlasmidImplanter(Pawn pawn)
        {
            if (pawn.genes.GenesListForReading.Any(gene => gene.def.GetModExtension<FPM_GeneExtension>()?.plasmidImplanter == true))
            {
                AbilityDef implantPlasmidDef = DefDatabase<AbilityDef>.GetNamed("ImplantPlasmid");

                if ((!pawn.abilities.abilities.Any(a => a.def == implantPlasmidDef)) && (pawn.genes.GenesListForReading.Any(gene => gene.def is PlasmidDef)))
                { pawn.abilities.GainAbility(implantPlasmidDef); }
                else
                { pawn.abilities.RemoveAbility(implantPlasmidDef); }
            }
        }

        public static void GeneRepair(Pawn pawn)
        {
            {
                foreach (Gene gene in pawn.genes.GenesListForReading)
                {
                    if ((gene.def is PlasmidDef plasmidDef) && (gene.def.GetModExtension<FPM_GeneExtension>()?.geneRepair == true))
                    {
                        var geneSet = plasmidDef.geneSet;
                        if (geneSet != null)
                        {
                            foreach (GeneDef geneDef in plasmidDef.geneSet)
                            {
                                if (!pawn.genes.HasActiveGene(geneDef))
                                {
                                    EBSGFramework.EBSGUtilities.AddGenesToPawn(pawn, plasmidDef.xenogene, null, geneDef);
                                    return;
                                }
                            }

                            foreach (GeneDef geneDef in plasmidDef.removedGenes)
                            {
                                if (pawn.genes.HasActiveGene(geneDef))
                                {
                                    EBSGFramework.EBSGUtilities.RemoveGenesFromPawn(pawn, plasmidDef.removedGenes);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public static class GeneExtensionMethods
    {
        public static List<FPM_GeneExtension> GetActiveGeneExtensions(this Pawn_GeneTracker geneTracker)
        {
            var gExtensions = geneTracker?.GenesListForReading?
                .Select(gene => gene.def.GetModExtension<FPM_GeneExtension>())
                .Where(extension => extension != null)
                .ToList();
            return gExtensions ?? new List<FPM_GeneExtension>();
        }
    }
}