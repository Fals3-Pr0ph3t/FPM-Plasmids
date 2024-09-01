using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Diagnostics;
using Verse;

namespace FPMPlasmids
{
    public class CompAbilityEffect_ImplantPlasmids : CompAbilityEffect
    {
        //public new AbilityCompProperties_ImplantPlasmids Props => (AbilityCompProperties_ImplantPlasmids)props;

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

            // If there's only one plasmid, apply it directly
            if (genePlasmids.Count == 1)
            {
                // Add genes to the target pawn
                EBSGFramework.EBSGUtilities.AddGenesToPawn(target.Pawn, genePlasmids.Values.First().xenogene, genePlasmids.Values.First().geneSet);
                // Transfer genes if specified
                if (genePlasmids.Values.First().transfer)
                {
                    EBSGFramework.EBSGUtilities.RemoveGenesFromPawn(casterPawn, genePlasmids.Values.First().geneSet);
                }
                // Remove genes from the caster pawn if specified
                if (genePlasmids.Values.First().removedGenes != null && genePlasmids.Values.First().removedGenes.Any())
                {
                    EBSGFramework.EBSGUtilities.RemoveGenesFromPawn(casterPawn, genePlasmids.Values.First().removedGenes);
                }
                isPlasmidSelected = true;
                return;
            }

            // Create a list of menu options for the float menu
            
            foreach (var plasmid in genePlasmids)
            {
                // Placeholder: Define the action to be performed when this option is selected
                Action selectAction = () =>
                {
                    // Add genes to the target pawn
                    EBSGFramework.EBSGUtilities.AddGenesToPawn(target.Pawn, genePlasmids.Values.First().xenogene, genePlasmids.Values.First().geneSet);
                    // Transfer genes if specified
                    if (genePlasmids.Values.First().transfer)
                    {
                        EBSGFramework.EBSGUtilities.RemoveGenesFromPawn(casterPawn, genePlasmids.Values.First().geneSet);
                    }
                    // Remove genes from the caster pawn if specified
                    if (genePlasmids.Values.First().removedGenes != null && genePlasmids.Values.First().removedGenes.Any())
                    {
                        EBSGFramework.EBSGUtilities.RemoveGenesFromPawn(casterPawn, genePlasmids.Values.First().removedGenes);
                    }
                    isPlasmidSelected = true;
                };

                // Create a menu option and add it to the list
                menuOptions.Add(new FloatMenuOption(plasmid.Key, selectAction));
            }

            // Create and display the float menu
            FloatMenu floatMenu = new FloatMenu(menuOptions)
            {
                vanishIfMouseDistant = false,
                forcePause = true,
                onCloseCallback = () =>
                {
                    // If no xenotype was selected, reset the ability cooldown
                    if (!isPlasmidSelected)
                    {
                        Messages.Message("No xenotype selected.", MessageTypeDefOf.RejectInput, true);
                        this.parent.ResetCooldown();
                    }
                }
            };
            Find.WindowStack.Add(floatMenu);
        }












        /*HediffDef loss = DefDatabase<HediffDef>.GetNamed("XenogermLossShock");
            if (casterPawn.health.hediffSet.TryGetHediff(loss, out Hediff hediff))
            {
                return;
            }
            casterPawn.health.AddHediff(loss);*/
        }
    }
