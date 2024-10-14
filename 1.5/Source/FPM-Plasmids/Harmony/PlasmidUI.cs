using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;

namespace FPMPlasmids
{
    // Patch to modify the gene icons directly without altering the categories
    [HarmonyPatch(typeof(GeneUIUtility), "DrawGene")]
    public static class GeneUIUtility_DrawGene_Patch
    {
        // Postfix to modify the gene icons after the original drawing logic
        public static void Postfix(Gene gene, Rect geneRect, GeneType geneType, bool doBackground, bool clickable)
        {
            // Check if the gene has the plasmid mod extension or if it is flagged as a mod gene
            if (gene.def.GetModExtension<FPM_GeneExtension>()?.plasmidInfo != null || FPM_Utils.GetPlasmidFlag(gene) == PlasmidFlag.Supressed || FPM_Utils.GetPlasmidFlag(gene) == PlasmidFlag.Created)
            {
                // Load and draw the custom icon on top of the gene
                Texture2D customIcon = ContentFinder<Texture2D>.Get("UI/Icons/Genes/Plasmid");

                // Draw the custom icon above the background but below the gene icon
                GUI.DrawTexture(new Rect(geneRect.x + 2, geneRect.y + 2, 30, 30), customIcon);
            }
        }
    }
}