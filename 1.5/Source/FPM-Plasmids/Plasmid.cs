using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using RimWorld.SketchGen;

namespace FPMPlasmids
{
    public class PlasmidDef : GeneDef
    {
        public bool xenogene = false;
        public bool transfer = true;
        public List<GeneDef> geneSet = new List<GeneDef>();                   // Declare the list
        public List<GeneDef> removedGenes = new List<GeneDef>();              // Declare the list

        public override void PostLoad()
        {
            base.PostLoad();
            LongEventHandler.ExecuteWhenFinished(delegate ()
                {
                    GeneList(this);
                    string GeneList(PlasmidDef plasmidDef)
                    {
                        if (plasmidDef.geneSet == null || plasmidDef.geneSet.Count == 0 && plasmidDef.defName != null)
                        {
                            Log.Error($"{plasmidDef}'s gene set is empty or null.");
                            return string.Empty;
                        }

                        StringBuilder sb = new StringBuilder();

                        foreach (var gene in plasmidDef.geneSet)
                        {
                            sb.Append(gene.label);
                            sb.Append(", ");
                        }

                        // Remove the last comma and space
                        if (sb.Length > 2)
                        {
                            sb.Length -= 2;
                        }
                        string result = sb.ToString();
                        plasmidDef.description = plasmidDef.description + "\nPlasmid contains the following genes:\n" + result;
                        return result;
                    }
                });
        }
    }
}







