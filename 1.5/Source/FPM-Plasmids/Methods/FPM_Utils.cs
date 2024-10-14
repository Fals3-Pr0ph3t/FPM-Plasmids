using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace FPMPlasmids
{
    public static class FPM_Utils
    {
        // Modifies a pawn's genes based on plasmid state (contained/expressed)
        public static void PlasmidModify(Pawn pawn, Plasmid plasmid)
        {
            var extension = plasmid.def.GetModExtension<FPM_GeneExtension>();
            var plasmidInfo = extension.plasmidInfo;

            if (plasmid.Contained == false)
            {
                plasmid.Contained = true;
                FPM_Utils.PlasmidRepair(pawn);
                return;
            }

            if (plasmid.Contained == true)
            {
                plasmid.Contained = false;
                FPM_Utils.PlasmidExpress(pawn, plasmid);
                return;
            }
        }

        // Repairs or modifies the pawn's genes based on the current plasmid state
        public static void PlasmidRepair(Pawn pawn)
        {
            // Dictionary for suppressed genes and list for added genes
            Dictionary<GeneDef, Plasmid> supp = new Dictionary<GeneDef, Plasmid>();
            List<GeneDef> add = new List<GeneDef>();

            // Loop through the pawn's genes to identify plasmid-related genes
            foreach (Gene g in pawn.genes.GenesListForReading)
            {
                if (g is Plasmid && g.Active)
                {
                    var plasmidInfo = g.def.GetModExtension<FPM_GeneExtension>().plasmidInfo;
                    add.AddRange(plasmidInfo.addedGenes);  // Add genes affected by the plasmid
                    foreach (GeneDef gd in plasmidInfo.suppressedGenes)
                    {
                        supp[gd] = g as Plasmid;  // Track suppressed genes and their associated plasmids
                    }
                }
            }

            add = add.Distinct().ToList();  // Remove duplicate genes in the add list

            // Handle gene removal and suppression
            foreach (Gene g in pawn.genes.GenesListForReading)
            {
                var state = FPM_Utils.GetPlasmidFlag(g);

                // Remove gene if it shouldn't be in the pawn's gene list
                if (state == PlasmidFlag.Created && !add.Contains(g.def))
                {
                    pawn.genes.RemoveGene(g);
                }

                // Override gene if it's suppressed
                if (state == PlasmidFlag.Supressed)
                {
                    if (supp.ContainsKey(g.def))
                    {
                        g.overriddenByGene = supp[g.def]; // Override the gene
                    }
                    else
                    {
                        g.overriddenByGene = null;  // Remove override if not suppressed
                        supp.Remove(g.def);
                    }
                }
            }
        }

        // Expresses genes linked to the plasmid in the pawn
        public static void PlasmidExpress(Pawn pawn, Plasmid plasmid)
        {
            var extension = plasmid.def.GetModExtension<FPM_GeneExtension>();
            var plasmidInfo = extension.plasmidInfo;
            if (extension == null || extension.plasmidInfo == null)
            {
                Log.Warning($"Plasmid {plasmid.def.label} has no valid extension or plasmid info.");
                return;
            }
            // Process added genes
            if (plasmidInfo.addedGenes != null && plasmid.Contained == false)
            {
                foreach (GeneDef g in plasmidInfo.addedGenes)
                {
                    if (!pawn.genes.HasActiveGene(g))
                    {
                        FPM_Utils.PlasmidAdd(pawn, plasmid);  // Add the gene if it's not active
                    }
                }
            }

            // Process suppressed genes
            if (plasmidInfo.suppressedGenes != null && plasmid.Contained == false)
            {
                foreach (GeneDef g in plasmidInfo.suppressedGenes)
                {
                    if (pawn.genes.HasActiveGene(g))
                    {
                        FPM_Utils.PlasmidSuppress(pawn, plasmid);  // Suppress the gene if it's active
                    }
                }
            }
        }

        // Adds a plasmid gene to the pawn's gene list

        public static void PlasmidAdd(Pawn pawn, Plasmid plasmid)
        {
            bool xenogene = plasmid.def.GetModExtension<FPM_GeneExtension>().plasmidInfo.xenogene;
            foreach (GeneDef g in plasmid.def.GetModExtension<FPM_GeneExtension>().plasmidInfo.addedGenes)
            {
                if (!pawn.genes.HasActiveGene(g))
                {
                    pawn.genes.AddGene(g, xenogene);
                    Gene ge = pawn.genes.GenesListForReading.FirstOrDefault(a => a.def == g);
                    FPM_Utils.SetPlasmidFlag(ge, PlasmidFlag.Created);
                }
            }
        }

        // Suppresses a plasmid gene by overriding it
        public static void PlasmidSuppress(Pawn pawn, Gene plasmid)
        {
            foreach (GeneDef g in plasmid.def.GetModExtension<FPM_GeneExtension>().plasmidInfo.suppressedGenes)
                if (pawn.genes.HasActiveGene(g))
                {
                    Gene gene = pawn.genes.GenesListForReading.FirstOrDefault(ge => ge.def == g);
                    if (gene.Active)
                    {
                        //gene.OverrideBy(plasmid);
                        FPM_Utils.SetPlasmidFlag(gene, PlasmidFlag.Supressed);
                        var flag = FPM_Utils.GetPlasmidFlag(gene);
                    }
                }
        }

        public static void PlasmidApply(Pawn caster, Pawn target, Plasmid plasmid)
        {
            if (plasmid.Contained == true)
            {
                Messages.Message($"{plasmid.def.label.CapitalizeFirst()} is currently contained, and can't be transfered.", MessageTypeDefOf.RejectInput, false);
                return;
            }
            PlasmidInfo plasmidInfo = plasmid.def.GetModExtension<FPM_GeneExtension>().plasmidInfo;
            FPM_Utils.PlasmidExpress(target, plasmid);
            FPM_Utils.PlasmidSuppress(target, plasmid);

            switch (plasmidInfo.transferType)
            {
                case TransferType.Transfer:
                    Log.Message("starting plasmid transfer");
                    target.genes.AddGene(plasmid.def, plasmidInfo.xenogene);
                    FPM_Utils.PlasmidModify(caster, plasmid);
                    caster.genes.RemoveGene(plasmid);
                    break;

                case TransferType.Copy:
                    Log.Message("starting plasmid copy");
                    target.genes.AddGene(plasmid.def, plasmidInfo.xenogene);
                    break;

                case TransferType.Apply:

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // Manages the ability to implant plasmids based on the pawn's genes
        public static void PlasmidImplanter(Pawn pawn)
        {
            if (pawn.genes.GenesListForReading.Any(gene => gene.def.GetModExtension<FPM_GeneExtension>()?.plasmidImplanter == true))
            {
                AbilityDef implantPlasmidDef = DefDatabase<AbilityDef>.GetNamed("ImplantPlasmid");

                if (!pawn.abilities.abilities.Any(a => a.def == implantPlasmidDef) &&
                    pawn.genes.GenesListForReading.Any(gene => gene.def.GetModExtension<FPM_GeneExtension>()?.plasmidInfo != null))
                {
                    pawn.abilities.GainAbility(implantPlasmidDef);
                }
                else
                {
                    pawn.abilities.RemoveAbility(implantPlasmidDef);
                }
            }
        }

        // Retrieves all plasmid genes the pawn possesses
        public static List<Plasmid> GetPlasmids(Pawn pawn)
        {
            List<Plasmid> plasmids = new List<Plasmid>();

            if (pawn == null || pawn.genes == null)
            {
                Messages.Message($"{pawn.Name} does not have any valid genes.", MessageTypeDefOf.RejectInput, false);
                return null;
            }

            foreach (Gene gene in pawn.genes.GenesListForReading)
            {
                FPM_GeneExtension extension = gene.def.GetModExtension<FPM_GeneExtension>();
                if (extension?.plasmidInfo != null)
                {
                    plasmids.Add(gene as Plasmid);
                }
            }

            return plasmids;
        }

        // Displays a floating menu for selecting plasmids
        public static GeneDef ShowPlasmidFloatMenu(List<Plasmid> plasmids, Action<GeneDef> onGeneSelected)
        {
            GeneDef selectedDef = null;
            if (plasmids == null || plasmids.Count == 0)
            {
                Messages.Message("No valid plasmids.", MessageTypeDefOf.RejectInput, false);
                return null;
            }

            List<FloatMenuOption> options = new List<FloatMenuOption>();

            foreach (Gene gene in plasmids)
            {
                GeneDef currentGeneDef = gene.def;

                Action selectAction = () =>
                {
                    selectedDef = currentGeneDef;  // Invoke callback
                };

                options.Add(new FloatMenuOption(gene.def.label.CapitalizeFirst(), delegate
                {
                    onGeneSelected?.Invoke(currentGeneDef);
                }));
            }

            FloatMenu floatMenu = new FloatMenu(options)
            {
                vanishIfMouseDistant = true,
                forcePause = true,
            };

            Find.WindowStack.Add(floatMenu);

            return selectedDef;
        }

        // Static class for managing the icons used in the UI for plasmids
        [StaticConstructorOnStartup]
        public static class FPM_Icon
        {
            public static Texture2D plasmidIcon;

            static FPM_Icon()
            {
                // Load the plasmid icon
                plasmidIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/TransferPlasmid", true);
            }
        }

        // Debug tool for repairing plasmids
        [DebugAction("Pawns", null, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresBiotech = true, displayPriority = 1000)]
        private static void PlasmidRepairTool(Pawn p)
        {
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(Options_RepairPlasmids(p)));
        }

        // Options list for selecting plasmids to repair in the debug tool
        public static List<DebugMenuOption> Options_RepairPlasmids(Pawn pawn)
        {
            List<DebugMenuOption> list = new List<DebugMenuOption>();
            List<Plasmid> plasmidList = new List<Plasmid>();

            if (pawn.genes != null)
            {
                plasmidList = FPM_Utils.GetPlasmids(pawn).ToList();

                foreach (Gene gene in plasmidList)
                {
                    if (gene is Plasmid)
                    {
                        list.Add(new DebugMenuOption(gene.LabelCap, DebugMenuOptionMode.Action, delegate
                        {
                            FPM_Utils.PlasmidModify(pawn, gene as Plasmid);
                        }
                    ));
                    }
                }
            }
            return list;
        }

        public static void SetPlasmidFlag(Gene gene, PlasmidFlag flag)
        {
            PlasmidGameComponent plasmidComponent = Current.Game.GetComponent<PlasmidGameComponent>();
            plasmidComponent.SetPlasmidFlag(gene, flag);
        }

        public static PlasmidFlag? GetPlasmidFlag(Gene gene)
        {
            PlasmidGameComponent plasmidComponent = Current.Game.GetComponent<PlasmidGameComponent>();
            return plasmidComponent.GetPlasmidFlag(gene);
        }

        public static int PlasmidComplexity(Plasmid plasmid)
        {
            var complex = 0;
            PlasmidInfo p = plasmid.def.GetModExtension<FPM_GeneExtension>().plasmidInfo;
            foreach (GeneDef g in p.addedGenes)
            {
                complex += g.biostatCpx;
            }
            foreach (GeneDef g in p.suppressedGenes)
            {
                complex += g.biostatCpx;
            }
            return complex;
        }

        public static void PlasmidSeverity(Plasmid plasmid, Hediff hediff)
        {
            int? complexity = PlasmidComplexity((Plasmid)plasmid);
            float? factor = hediff.def.CompProps<FPMPlasmids.HediffCompProperties_PlasmidSeverityPerDay>().severityFactor;
            if (complexity == null || factor == null)
            {
                Log.Error("Null on either severity factor or complexity");
            }
            Log.Message($"the factor reads as {factor}");
            var limit = (complexity * factor);
            if (limit == 0 || limit == null)
            { limit = 1f; }
            hediff.def.CompProps<HediffCompProperties_PlasmidSeverityPerDay>().severityPerDay = (float)(1f / (complexity * factor));

            return;
        }
    }

    public enum PlasmidFlag
    {
        Created,
        Supressed,
    }

    public class PlasmidGameComponent : GameComponent
    {
        // Dictionary to store the PlasmidFlag for each Gene
        private Dictionary<String, PlasmidFlag?> plasmidFlags = new Dictionary<String, PlasmidFlag?>();

        // Parameterless constructor required for GameComponent instantiation
        public PlasmidGameComponent(Game game)
        {
            plasmidFlags = new Dictionary<String, PlasmidFlag?>();
            Log.Message("PlasmidGameComponent Initialized");
        }

        // Set the PlasmidFlag for a Gene
        public void SetPlasmidFlag(Gene gene, PlasmidFlag flag)
        {
            if (gene != null)
            {
                plasmidFlags[gene.GetUniqueLoadID()] = flag;
            }
        }

        // Get the PlasmidFlag for a Gene
        public PlasmidFlag? GetPlasmidFlag(Gene gene)
        {
            return plasmidFlags.TryGetValue(gene.GetUniqueLoadID(), out var flag) ? flag : null;
        }

        // ExposeData for saving and loading the dictionary
        public override void ExposeData()
        {
            base.ExposeData();
            Log.Message("ExposeData called.");

            // Lists to store keys and values during saving/loading
            List<string> geneKeys = plasmidFlags != null ? plasmidFlags.Keys.ToList() : null;
            List<PlasmidFlag?> plasmidFlagsValues = plasmidFlags != null ? plasmidFlags.Values.ToList() : null;

            // Correct LookMode for saving strings (LookMode.Value) and enum (LookMode.Value)
            Scribe_Collections.Look(ref plasmidFlags, "plasmidFlags", LookMode.Value, LookMode.Value, ref geneKeys, ref plasmidFlagsValues);

            // If we're loading, recreate the dictionary if necessary
            if (Scribe.mode == LoadSaveMode.LoadingVars && geneKeys != null && plasmidFlagsValues != null)
            {
                plasmidFlags = new Dictionary<string, PlasmidFlag?>();
                for (int i = 0; i < geneKeys.Count; i++)
                {
                    if (geneKeys[i] != null)
                    {
                        plasmidFlags[geneKeys[i]] = plasmidFlagsValues[i];
                    }
                }
                Log.Message($"Loaded {plasmidFlags.Count} plasmid flags.");
            }
        }
    }
}