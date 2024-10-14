using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using static FPMPlasmids.FPM_Utils;

namespace FPMPlasmids
{
    public class CompAbilityEffect_PlasmidImplanter : CompAbilityEffect
    {
        public new AbilityCompProperties_PlasmidImplanter Props => (AbilityCompProperties_PlasmidImplanter)props;

        /// A list to hold available plasmids for the current pawn.

        public List<Plasmid> plasmids = new();

        /// The currently selected plasmid that the player has chosen for implanting.

        public Plasmid plasmidChoice;

        /// Provides additional gizmos (UI elements) for the plasmid selection.
        /// Only shows the plasmid choice gizmo if the pawn has the "ImplantPlasmid" ability.

        /// Checks if the plasmid implant ability should be disabled, for example, due to the pawn recovering from
        /// xenogerm loss or not having a selected plasmid.
        public override bool GizmoDisabled(out string reason)
        {
            Pawn casterPawn = parent.pawn;
            HediffDef loss = DefDatabase<HediffDef>.GetNamed("XenogermLossShock");

            // Disable if the pawn is recovering from xenogerm loss.
            if (casterPawn.health.hediffSet.TryGetHediff(loss, out Hediff hediff))
            {
                reason = "genes are regrowing.";
                return true;
            }

            // Disable if no plasmid has been selected.
            //if (plasmidChoice == null)
            //{
            //    reason = "no plasmid selected";
            //    return true;
            //}

            // Otherwise, check base class logic.
            return base.GizmoDisabled(out reason);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn pawn = target.Pawn;
            if (pawn == null)
            {
                return base.Valid(target, throwMessages);
            }
            //if (plasmidChoice == null)
            //{
            //    if (throwMessages)
            //    {
            //        Messages.Message("No plasmid is selected.", pawn, MessageTypeDefOf.RejectInput, historical: false);
            //    }
            //    return false;
            //}
            if (pawn.IsQuestLodger())
            {
                if (throwMessages)
                {
                    Messages.Message("MessageCannotImplantInTempFactionMembers".Translate(), pawn, MessageTypeDefOf.RejectInput, historical: false);
                }
                return false;
            }
            if (pawn.HostileTo(parent.pawn) && !pawn.Downed)
            {
                if (throwMessages)
                {
                    Messages.Message("MessageCantUseOnResistingPerson".Translate(parent.def.Named("ABILITY")), pawn, MessageTypeDefOf.RejectInput, historical: false);
                }
                return false;
            }
            return base.Valid(target, throwMessages);
        }

        public override Window ConfirmationDialog(LocalTargetInfo target, Action confirmAction)
        {
            // Create a list of FloatMenuOptions for each available plasmid
            List<FloatMenuOption> options = new();

            // Get the plasmids for the current pawn
            List<Plasmid> plasmids = FPM_Utils.GetPlasmids(parent.pawn);

            if (plasmids != null && plasmids.Count > 0)
            {
                foreach (Plasmid plasmid in plasmids)
                {
                    GeneDef geneDef = plasmid.def;
                    options.Add(new FloatMenuOption(geneDef.label, () =>
                    {
                        // On selection of a plasmid, apply it
                        this.plasmidChoice = plasmid;
                        confirmAction.Invoke(); // Proceed with the ability after the selection
                    }));
                }

                FloatMenu floatMenu = new(options)
                {
                    vanishIfMouseDistant = false,
                    forcePause = true,
                };

                // Show the float menu
                Find.WindowStack.Add(floatMenu);
            }
            else
            {
                Messages.Message("No plasmids available.", MessageTypeDefOf.RejectInput, false);
            }

            // Return null because we're not using a traditional Window here
            return null;
        }

        /// Applies the selected plasmid's effects to the target pawn. Adds the plasmid's genes to the target,
        /// removes specified genes from the caster, and ensures no duplicate plasmids are implanted.

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            Pawn pawn = target.Pawn;

            if (plasmidChoice == null)
            {
                Log.Error("No plasmid selected");
                return;
            }

            foreach (var ability in caster.abilities.abilities)
            {
                Log.Message($"{ability.GetType()}");
            }

            // Ensure the caster has plasmids available.
            if (!caster.genes.GenesListForReading.Any(g => g.def.GetModExtension<FPM_GeneExtension>()?.plasmidInfo != null))
            {
                Messages.Message($"{caster.Name} doesn't have any plasmids", MessageTypeDefOf.RejectInput, false);
                return;
            }

            // Show float menu and use a callback to handle the rest of the logic after a plasmid is selected

            FPM_GeneExtension extension = plasmidChoice.def.GetModExtension<FPM_GeneExtension>();
            var plasmid = extension.plasmidInfo;

            // Check if the target already has the chosen plasmid
            if (target == null || target.Pawn.genes.HasActiveGene(plasmidChoice.def))
            {
                Messages.Message($"{target.Pawn.Name} already possesses the chosen plasmid.", MessageTypeDefOf.RejectInput, false);
                parent.ResetCooldown();  // Reset the ability cooldown if plasmid is already present
                return;
            }
            else
            {
                Log.Message("Applying Plasmid");
                FPM_Utils.PlasmidApply(parent.pawn, target.Pawn, plasmidChoice);

                Log.Message("Plasmid applied, adding plasmid flu.");
                Hediff plasmidFlu = HediffMaker.MakeHediff(HediffDefOf.PlasmidFlu, target.Pawn);
                FPM_Utils.PlasmidSeverity(plasmidChoice, plasmidFlu);
                target.Pawn.health.AddHediff(plasmidFlu);

                Log.Message("flu applied, adding regrow.");
                Hediff plasmidRegrow = HediffMaker.MakeHediff(HediffDefOf.PlasmidRegrow, parent.pawn);
                FPM_Utils.PlasmidSeverity(plasmidChoice, plasmidRegrow);
                parent.pawn.health.AddHediff(plasmidRegrow);
            }
        }
    }
}