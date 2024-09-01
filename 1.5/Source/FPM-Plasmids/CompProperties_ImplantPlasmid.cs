using System.Collections.Generic;
using RimWorld;
namespace FPMPlasmids
{
    public class AbilityCompProperties_ImplantPlasmid : CompProperties_AbilityEffect
    {
        public List<PlasmidDef> plasmidDefs;
        public AbilityCompProperties_ImplantPlasmid()
        {
            compClass = typeof(CompAbilityEffect_ImplantPlasmid);
        }
    }
}