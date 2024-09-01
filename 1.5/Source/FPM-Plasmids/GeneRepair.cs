using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using static HarmonyLib.Code;
using static UnityEngine.GraphicsBuffer;

namespace FPMPlasmids
{
    public class RepairGene : Gene
    {
        public override void Tick()
        {

            base.Tick();

            if (pawn.IsHashIntervalTick(180))
            {

                foreach (Gene gene in pawn.genes.GenesListForReading)
                {
                    if (gene.def.GetType() == typeof(PlasmidDef))
                    {
                        PlasmidDef plasmidDef = gene.def as PlasmidDef;
                        var geneSet = plasmidDef.geneSet;
                        if (geneSet != null)
                        {
                            foreach (GeneDef geneDef in plasmidDef.geneSet)
                                if (!pawn.genes.HasActiveGene(geneDef))
                                { EBSGFramework.EBSGUtilities.AddGenesToPawn(pawn, plasmidDef.xenogene, null, geneDef);
                                    //await Task.Delay(3000);
                                }
                            foreach (GeneDef geneDef in plasmidDef.removedGenes)
                                if (pawn.genes.HasActiveGene(geneDef))
                                { EBSGFramework.EBSGUtilities.RemoveGenesFromPawn(pawn, plasmidDef.removedGenes);
                                    //await Task.Delay(3000);
                                }
                        }
                    }
                }
            }

        }
    }
}

