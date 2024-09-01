using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace FPMPlasmids
{
    public class CompAbilityEffect_ImplantPlasmid : CompAbilityEffect
    {
        public new AbilityCompProperties_ImplantPlasmid Props => (AbilityCompProperties_ImplantPlasmid)props;

        public override bool GizmoDisabled(out string reason)
        {
            Pawn casterPawn = parent.pawn;
            HediffDef loss = DefDatabase<HediffDef>.GetNamed("XenogermLossShock");
            if (casterPawn.health.hediffSet.TryGetHediff(loss, out Hediff hediff))
            {
                reason = ("genes are regrowing.");
                return true;
            }


            return base.GizmoDisabled(out reason);
        }
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn casterPawn = parent.pawn;


            // Ensure we are working with a valid target
            if (target.Pawn == null)
            {
                Log.Error("Target pawn is null.");
                return;
            }
            // This is the pawn casting the ability


            if (casterPawn == null)
            {
                Log.Error("Caster pawn is null.");
                return;
            }

            HediffDef loss = DefDatabase<HediffDef>.GetNamed("XenogermLossShock");
            if (casterPawn.health.hediffSet.TryGetHediff(loss, out Hediff hediff))
            {
                return;
            }

            foreach (var plasmidDef in Props.plasmidDefs)
            {
                {
                    // Add genes to the target pawn
                    EBSGFramework.EBSGUtilities.AddGenesToPawn(target.Pawn, plasmidDef.xenogene, plasmidDef.geneSet);
                    // Transfer genes if specified
                    if (plasmidDef.transfer)
                    {
                        EBSGFramework.EBSGUtilities.RemoveGenesFromPawn(casterPawn, plasmidDef.geneSet);
                    }
                    // Remove genes from the caster pawn if specified
                    if (plasmidDef.removedGenes != null && plasmidDef.removedGenes.Any())
                    {
                        EBSGFramework.EBSGUtilities.RemoveGenesFromPawn(casterPawn, plasmidDef.removedGenes);
                    }
                }
            }
            casterPawn.health.AddHediff(loss);
        }
    }
}