using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FPMPlasmids
{
    public static class FPM_Utils
    {
        public static Dictionary<string, PlasmidDef> GetGenePlasmids(Pawn pawn)
        {
            Dictionary<string, PlasmidDef> dictionary = new Dictionary<string, PlasmidDef>();
            foreach (Gene g in pawn.genes.GenesListForReading)
            {
                if ((g.def is PlasmidDef))
                {
                    dictionary.Add(g.def.label,g.def as PlasmidDef);
                }
            }
            return dictionary;
        }
    }
}