using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Diagnostics;
using Verse;

namespace FPMPlasmids
{
    public class CompAbilityEffect_PlasmidImplanter : CompAbilityEffect
    {
        public new AbilityCompProperties_PlasmidImplanter Props => (AbilityCompProperties_PlasmidImplanter)props;

        private bool isPlasmidSelected;
        private List<FloatMenuOption> menuOptions = new List<FloatMenuOption>();




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




            Dictionary<string, PlasmidDef> genePlasmids = FPM_Utils.GetGenePlasmids(casterPawn);
            if (genePlasmids == null || genePlasmids.Count == 0)
            {
                // Show a message if no plasmids are available
                Messages.Message("No plasmids available.", MessageTypeDefOf.RejectInput, true);
                return;
            }

            

            // Create a list of menu options for the float menu

            foreach (KeyValuePair<string, PlasmidDef> kvp in genePlasmids)
            {
                if (target.Pawn.genes.HasActiveGene(kvp.Value as GeneDef)){ continue; }
                
                // Placeholder: Define the action to be performed when this option is selected
                Action selectAction = () =>
                {
                    // Add genes to the target pawn
                    EBSGFramework.EBSGUtilities.AddGenesToPawn(target.Pawn, kvp.Value.xenogene, kvp.Value.geneSet);
                    // Transfer genes if specified
                    if (genePlasmids.Values.First().transfer)
                    {
                        EBSGFramework.EBSGUtilities.RemoveGenesFromPawn(casterPawn, kvp.Value.geneSet);
                    }
                    // Remove genes from the caster pawn if specified
                    if (kvp.Value.removedGenes != null && kvp.Value.removedGenes.Any())
                    {
                        EBSGFramework.EBSGUtilities.RemoveGenesFromPawn(casterPawn, kvp.Value.removedGenes);
                    }
                    isPlasmidSelected = true;

                    HediffDef loss = DefDatabase<HediffDef>.GetNamed("XenogermLossShock");
                    if (casterPawn.health.hediffSet.TryGetHediff(loss, out Hediff hediff)) { return; }
                    else { casterPawn.health.AddHediff(loss); }
                };

                // Create a menu option and add it to the list
                menuOptions.Add(new FloatMenuOption(kvp.Key.CapitalizeFirst(), selectAction));
            }

            // Create and display the float menu
            FloatMenu floatMenu = new FloatMenu(menuOptions)
            {
                vanishIfMouseDistant = false,
                forcePause = true,
                onCloseCallback = () =>
                {
                    // If no plasmid was selected, reset the ability cooldown
                    if (!isPlasmidSelected)
                    {
                        Messages.Message("No plasmid selected.", MessageTypeDefOf.RejectInput, true);
                        this.parent.ResetCooldown();
                    }
                }
            };
            Find.WindowStack.Add(floatMenu);
            floatMenu = null;
        }
    }
}